using OpenH2.Serialization.Materialization;
using OpenH2.Serialization.Metadata;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace OpenH2.Serialization
{
    public class BlamSerializer
    {
        private static ConcurrentDictionary<Type, (Delegate generic, Deserializer referenceType)> deserializers 
            = new ConcurrentDictionary<Type, (Delegate generic, Deserializer referenceType)>();
        private readonly Memory<byte> data;
        private readonly IInternedStringProvider stringProvider;

        static BlamSerializer()
        {
            Initialize();
        }

        static void Initialize()
        {
            var assys = AppDomain.CurrentDomain.GetAssemblies();

            foreach(var assy in assys)
            {
                var types = assy.GetTypes();

                foreach(var type in types)
                {
                    var serializationClass = type.GetCustomAttribute<SerializationClassAttribute>();

                    if(serializationClass != null)
                    {
                        var deserializeMethod = type.GetMethod(
                            SerializationClassAttribute.DeserializeMethod, 
                            BindingFlags.Public | BindingFlags.Static);

                        if(deserializeMethod != null)
                        {
                            Deserializer referenceTypeDelegate;

                            if(deserializeMethod.ReturnType.IsValueType == false)
                            {
                                referenceTypeDelegate = (Deserializer)deserializeMethod.CreateDelegate(typeof(Deserializer));
                            }
                            else
                            {
                                // If we're deserializing a struct here, we can't rely on variance to turn the
                                // value type into an object. Instead we can create an expression to do this

                                var parameters = new[] { typeof(Span<byte>), typeof(int), typeof(int), typeof(IInternedStringProvider) }
                                    .Select(Expression.Parameter).ToArray();
                                
                                var call = Expression.Convert(Expression.Call(null, deserializeMethod, parameters), typeof(object));
                                referenceTypeDelegate = Expression.Lambda<Deserializer>(call, parameters).Compile();
                            }

                            var deserializerType = typeof(Deserializer<>).MakeGenericType(deserializeMethod.ReturnType);
                            deserializers.TryAdd(deserializeMethod.ReturnType, (deserializeMethod.CreateDelegate(deserializerType), referenceTypeDelegate));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Instance method to provide args to always use, just dispatches to static with those args
        /// </summary>
        public BlamSerializer(Memory<byte> data, IInternedStringProvider stringProvider = null)
        {
            this.data = data;
            this.stringProvider = stringProvider;
        }

        public T Deserialize<T>(int instanceStart = 0, int staticOffset = 0)
        {
            return Deserialize<T>(this.data.Span, instanceStart, staticOffset, stringProvider);
        }

        public static object Deserialize(Type type, Span<byte> allData, int instanceStart = 0, int staticOffset = 0, IInternedStringProvider stringProvider = null)
        {
            if (deserializers.TryGetValue(type, out var desers) && desers.referenceType != null)
            {
                return desers.referenceType(allData, instanceStart, staticOffset, stringProvider);
            }

            throw new NotSupportedException($"Type '{type.Name}' was not found in the deserializer collection");
        }

        public static T Deserialize<T>(Span<byte> allData, int instanceStart = 0, int staticOffset = 0, IInternedStringProvider stringProvider = null)
        {
            if (deserializers.TryGetValue(typeof(T), out var deser))
            {
                return ((Deserializer<T>)deser.generic)(allData, instanceStart, staticOffset, stringProvider);
            }

            throw new NotSupportedException($"Type '{typeof(T).Name}' was not found in the deserializer collection");
        }
    }
}
