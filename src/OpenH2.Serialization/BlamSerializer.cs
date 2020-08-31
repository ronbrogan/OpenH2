using OpenH2.Serialization.Materialization;
using OpenH2.Serialization.Metadata;
using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace OpenH2.Serialization
{
    public class BlamSerializer
    {
        private static ConcurrentDictionary<Type, Deserializer> deserializers = new ConcurrentDictionary<Type, Deserializer>();
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
                            deserializers.TryAdd(deserializeMethod.ReturnType, (Deserializer)deserializeMethod.CreateDelegate(typeof(Deserializer)));
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
            if (deserializers.TryGetValue(type, out var deser))
            {
                return deser(allData, instanceStart, staticOffset, stringProvider);
            }

            throw new NotSupportedException($"Type '{type.Name}' was not found in the deserializer collection");
        }

        public static T Deserialize<T>(Span<byte> allData, int instanceStart = 0, int staticOffset = 0, IInternedStringProvider stringProvider = null)
        {
            return (T)Deserialize(typeof(T), allData, instanceStart, staticOffset, stringProvider);
        }
    }
}
