using OpenH2.Serialization.Materialization;
using OpenH2.Serialization.Metadata;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace OpenH2.Serialization
{
    public class BlamSerializer
    {
        private static ConcurrentDictionary<Type, Deserializers> deserializers 
            = new ConcurrentDictionary<Type, Deserializers>();
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
                        var desers = new Deserializers();
                        var objType = PopulateDeserializers(type, useStreams: false, ref desers);
                        PopulateDeserializers(type, useStreams: true, ref desers);

                        deserializers[objType] = desers;
                    }
                }
            }
        }

        private static Type PopulateDeserializers(Type type, bool useStreams, ref Deserializers deserializers)
        {
            var dataType = useStreams ? typeof(Stream) : typeof(Span<byte>);

            var deserMethodParams = new[] { dataType, typeof(int), typeof(int), typeof(IInternedStringProvider) };
            var deserializeMethod = type.GetMethod(
                SerializationClassAttribute.DeserializeMethod,
                BindingFlags.Public | BindingFlags.Static,
                null,
                deserMethodParams,
                null);

            var objType = deserializeMethod.ReturnType;

            var deserIntoMethodParams = new[] { objType, dataType, typeof(int), typeof(int), typeof(IInternedStringProvider) };

            var deserializeIntoMethod = type.GetMethod(
                SerializationClassAttribute.DeserializeIntoMethod,
                BindingFlags.Public | BindingFlags.Static,
                null,
                deserIntoMethodParams,
                null);

            if (deserializeMethod != null && deserializeIntoMethod != null
                && deserializeMethod.ReturnType == deserializeIntoMethod.ReturnType)
            {
                if(useStreams)
                {
                    PopulateStreamDeserializers(objType, dataType, deserializeMethod, deserializeIntoMethod, ref deserializers);
                }
                else
                {
                    PopulateSpanDeserializers(objType, dataType, deserializeMethod, deserializeIntoMethod, ref deserializers);
                }
            }

            return objType;
        }

        private static void PopulateSpanDeserializers(Type objType, 
            Type dataType,
            MethodInfo deserializeMethod,
            MethodInfo deserializeIntoMethod, 
            ref Deserializers deserializers)
        {
            Deserializer referenceTypeDelegate;

            if (objType.IsValueType == false)
            {
                referenceTypeDelegate = (Deserializer)deserializeMethod.CreateDelegate(typeof(Deserializer));
            }
            else
            {
                // If we're deserializing a struct here, we can't rely on variance to turn the
                // value type into an object. Instead we can create an expression to do this

                var parameters = new[] { dataType, typeof(int), typeof(int), typeof(IInternedStringProvider) }
                    .Select(Expression.Parameter).ToArray();

                var call = Expression.Convert(Expression.Call(null, deserializeMethod, parameters), typeof(object));
                referenceTypeDelegate = Expression.Lambda<Deserializer>(call, parameters).Compile();
            }

            var intoParams = new[] { typeof(object), dataType, typeof(int), typeof(int), typeof(IInternedStringProvider) }
                    .Select(Expression.Parameter).ToArray();

            var intoCallArgs = new List<Expression>();
            intoCallArgs.Add(Expression.Convert(intoParams[0], objType));
            for (var i = 1; i < intoParams.Length; i++)
            {
                intoCallArgs.Add(intoParams[i]);
            }

            var intoCall = Expression.Convert(Expression.Call(null, deserializeIntoMethod, intoCallArgs), typeof(object));
            var intoReferenceTypeDelegate = Expression.Lambda<DeserializeInto>(intoCall, intoParams).Compile();

            var deserializerGenericType = typeof(Deserializer<>)
                .MakeGenericType(deserializeMethod.ReturnType);

            var deserializIntoGenericType = typeof(DeserializeInto<>)
                .MakeGenericType(deserializeIntoMethod.ReturnType);

            deserializers.deserializeGeneric = deserializeMethod.CreateDelegate(deserializerGenericType);
            deserializers.deserializeReference = referenceTypeDelegate;
            deserializers.deserializeIntoGeneric = deserializeIntoMethod.CreateDelegate(deserializIntoGenericType);
            deserializers.deserializeIntoReference = intoReferenceTypeDelegate;
        }

        private static void PopulateStreamDeserializers(Type objType,
            Type dataType,
            MethodInfo deserializeMethod,
            MethodInfo deserializeIntoMethod,
            ref Deserializers deserializers)
        {
            StreamDeserializer referenceTypeDelegate;

            if (objType.IsValueType == false)
            {
                referenceTypeDelegate = (StreamDeserializer)deserializeMethod.CreateDelegate(typeof(StreamDeserializer));
            }
            else
            {
                // If we're deserializing a struct here, we can't rely on variance to turn the
                // value type into an object. Instead we can create an expression to do this

                var parameters = new[] { dataType, typeof(int), typeof(int), typeof(IInternedStringProvider) }
                    .Select(Expression.Parameter).ToArray();

                var call = Expression.Convert(Expression.Call(null, deserializeMethod, parameters), typeof(object));
                referenceTypeDelegate = Expression.Lambda<StreamDeserializer>(call, parameters).Compile();
            }

            var intoParams = new[] { typeof(object), dataType, typeof(int), typeof(int), typeof(IInternedStringProvider) }
                    .Select(Expression.Parameter).ToArray();

            var intoCallArgs = new List<Expression>();
            intoCallArgs.Add(Expression.Convert(intoParams[0], objType));
            for (var i = 1; i < intoParams.Length; i++)
            {
                intoCallArgs.Add(intoParams[i]);
            }

            var intoCall = Expression.Convert(Expression.Call(null, deserializeIntoMethod, intoCallArgs), typeof(object));
            var intoReferenceTypeDelegate = Expression.Lambda<StreamDeserializeInto>(intoCall, intoParams).Compile();

            var deserializerGenericType = typeof(StreamDeserializer<>)
                .MakeGenericType(deserializeMethod.ReturnType);

            var deserializIntoGenericType = typeof(StreamDeserializeInto<>)
                .MakeGenericType(deserializeIntoMethod.ReturnType);

            deserializers.deserializeStreamGeneric = deserializeMethod.CreateDelegate(deserializerGenericType);
            deserializers.deserializeStreamReference = referenceTypeDelegate;
            deserializers.deserializeIntoStreamGeneric = deserializeIntoMethod.CreateDelegate(deserializIntoGenericType);
            deserializers.deserializeIntoStreamReference = intoReferenceTypeDelegate;
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
            if (deserializers.TryGetValue(type, out var desers) && desers.deserializeReference != null)
            {
                return desers.deserializeReference(allData, instanceStart, staticOffset, stringProvider);
            }

            throw new NotSupportedException($"Type '{type.Name}' was not found in the deserializer collection");
        }

        public static T Deserialize<T>(Span<byte> allData, int instanceStart = 0, int staticOffset = 0, IInternedStringProvider stringProvider = null)
        {
            if (deserializers.TryGetValue(typeof(T), out var deser))
            {
                return ((Deserializer<T>)deser.deserializeGeneric)(allData, instanceStart, staticOffset, stringProvider);
            }

            throw new NotSupportedException($"Type '{typeof(T).Name}' was not found in the deserializer collection");
        }

        public static object DeserializeInto(object instance, Type type, Span<byte> allData, int instanceStart = 0, int staticOffset = 0, IInternedStringProvider stringProvider = null)
        {
            if (deserializers.TryGetValue(type, out var desers) && desers.deserializeIntoReference != null)
            {
                return desers.deserializeIntoReference(instance, allData, instanceStart, staticOffset, stringProvider);
            }

            throw new NotSupportedException($"Type '{type.Name}' was not found in the deserializer collection");
        }

        public static T DeserializeInto<T>(T instance, Span<byte> allData, int instanceStart = 0, int staticOffset = 0, IInternedStringProvider stringProvider = null)
        {
            if (deserializers.TryGetValue(typeof(T), out var deser))
            {
                return ((DeserializeInto<T>)deser.deserializeIntoGeneric)(instance, allData, instanceStart, staticOffset, stringProvider);
            }

            throw new NotSupportedException($"Type '{typeof(T).Name}' was not found in the deserializer collection");
        }

        public static object Deserialize(Type type, Stream allData, int instanceStart = 0, int staticOffset = 0, IInternedStringProvider stringProvider = null)
        {
            if (deserializers.TryGetValue(type, out var desers) && desers.deserializeReference != null)
            {
                return desers.deserializeStreamReference(allData, instanceStart, staticOffset, stringProvider);
            }

            throw new NotSupportedException($"Type '{type.Name}' was not found in the deserializer collection");
        }

        public static T Deserialize<T>(Stream allData, int instanceStart = 0, int staticOffset = 0, IInternedStringProvider stringProvider = null)
        {
            if (deserializers.TryGetValue(typeof(T), out var deser))
            {
                return ((StreamDeserializer<T>)deser.deserializeStreamGeneric)(allData, instanceStart, staticOffset, stringProvider);
            }

            throw new NotSupportedException($"Type '{typeof(T).Name}' was not found in the deserializer collection");
        }

        public static object DeserializeInto(object instance, Type type, Stream allData, int instanceStart = 0, int staticOffset = 0, IInternedStringProvider stringProvider = null)
        {
            if (deserializers.TryGetValue(type, out var desers) && desers.deserializeIntoReference != null)
            {
                return desers.deserializeIntoStreamReference(instance, allData, instanceStart, staticOffset, stringProvider);
            }

            throw new NotSupportedException($"Type '{type.Name}' was not found in the deserializer collection");
        }

        public static T DeserializeInto<T>(T instance, Stream allData, int instanceStart = 0, int staticOffset = 0, IInternedStringProvider stringProvider = null)
        {
            if (deserializers.TryGetValue(typeof(T), out var deser))
            {
                return ((StreamDeserializeInto<T>)deser.deserializeIntoStreamGeneric)(instance, allData, instanceStart, staticOffset, stringProvider);
            }

            throw new NotSupportedException($"Type '{typeof(T).Name}' was not found in the deserializer collection");
        }

        private struct Deserializers
        {
            public Delegate deserializeGeneric;
            public Deserializer deserializeReference;

            public Delegate deserializeIntoGeneric;
            public DeserializeInto deserializeIntoReference;

            public Delegate deserializeStreamGeneric;
            public StreamDeserializer deserializeStreamReference;

            public Delegate deserializeIntoStreamGeneric;
            public StreamDeserializeInto deserializeIntoStreamReference;
        }
    }
}
