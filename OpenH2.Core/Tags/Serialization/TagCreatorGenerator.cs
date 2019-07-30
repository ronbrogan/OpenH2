using OpenH2.Core.Tags.Serialization.SerializerEmit;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace OpenH2.Core.Tags.Serialization
{
    public delegate object TagCreator(uint id, string name, Span<byte> data, int secondaryMagic, int startAt = 0);
    public delegate T TagCreator<T>(uint id, string name, Span<byte> data, int secondaryMagic, int startAt = 0);

    public class TagCreatorGenerator
    {
        
        private static Dictionary<Type, TagCreator> cachedTagCreatorDelegates = new Dictionary<Type, TagCreator>();

        private Func<Type, SerializerEmitContext> builderWrapperFactory;

        public TagCreatorGenerator()
        {
            Func<Type, SerializerEmitContext> nested = null;

            nested = (t) =>
            {
                var name = t.FullName
                  .Substring(t.FullName.LastIndexOf('.') + 1)
                  .Replace("+", "_");

                var method = new DynamicMethod("Read" + name, typeof(object), TagCreatorArguments.ArgumentTypes);

                return new SerializerEmitContext()
                {
                    MethodIL = method.GetILGenerator(),
                    GetNestedContext = nested,
                    GetMethodInfo = () => method
                };
            };

            builderWrapperFactory = nested;
        }

        public TagCreatorGenerator(ModuleBuilder module)
        {
            builderWrapperFactory = tagType =>
            {
                var type = module.DefineType($"{tagType.Name}Serializer", TypeAttributes.Public);

                var builder = type.DefineMethod("Read" + tagType.Name,
                    MethodAttributes.Public | MethodAttributes.Static,
                    typeof(object),
                    TagCreatorArguments.ArgumentTypes);

                Func<Type, SerializerEmitContext> nested = null;
                nested = (t) =>
                {
                    var name = t.FullName
                      .Substring(t.FullName.LastIndexOf('.') + 1)
                      .Replace("+", "_");

                    var methodBuilder = type.DefineMethod("Read" + name,
                        MethodAttributes.Public | MethodAttributes.Static,
                        typeof(object),
                        TagCreatorArguments.ArgumentTypes);

                    return new SerializerEmitContext()
                    {
                        MethodIL = methodBuilder.GetILGenerator(),
                        GetNestedContext = nested,
                        GetMethodInfo = () => methodBuilder
                    };
                };

                var wrapper = new SerializerEmitContext()
                {
                    MethodIL = builder.GetILGenerator(),
                    GetNestedContext = nested,
                    GetMethodInfo = () =>
                    {
                        var t = type.CreateTypeInfo().AsType();
                        return t.GetMethod(builder.Name);
                    }
                };

                return wrapper;
            };
        }

        public TagCreator<T> GetTagCreator<T>()
        {
            var tagType = typeof(T);

            var creator = GetTagCreator(tagType);

            return new TagCreator<T>((uint id, string name, Span<byte> s, int i, int o) => (T)creator(id, name, s, i, o));
        }

        public TagCreator GetTagCreator(Type tagType)
        {
            if (cachedTagCreatorDelegates.TryGetValue(tagType, out var deleg))
            {
                return deleg;
            }

            var context = builderWrapperFactory(tagType);

            ReaderEmitter.GenerateTagCreator(tagType, context);

            var creator = (TagCreator)context.GetMethodInfo().CreateDelegate(typeof(TagCreator));

            cachedTagCreatorDelegates.Add(tagType, creator);

            return creator;
        }
    }
}
