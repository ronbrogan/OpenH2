using OpenH2.Core.Offsets;
using OpenH2.Core.Parsing;
using OpenH2.Core.Representations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.Serialization;

namespace OpenH2.Core.Tags.Serialization.SerializerEmit
{
    internal static class TagCreatorArguments
    {
        public enum Name
        {
            Id,
            Name,
            Data,
            SecondaryMagic,
            StartAt,
            Length
        }

        public static SortedDictionary<Name, Type> ArgumentInfo = new SortedDictionary<Name, Type>
        {
            { Name.Id, typeof(uint) },
            { Name.Name, typeof(string) },
            { Name.Data, typeof(TrackingReader) },
            { Name.SecondaryMagic, typeof(int) },
            { Name.StartAt, typeof(int) },
            { Name.Length, typeof(int) }
        };

        public static Type[] ArgumentTypes = ArgumentInfo.Values.ToArray();
        public static Name[] ArgumentNames = ArgumentInfo.Keys.ToArray();

        public static int GetArgumentLocation(Name name)
        {
            return Array.IndexOf(ArgumentNames, name);
        }
    }

    internal static class MI
    {
        public static Dictionary<Type, MethodInfo> PrimitiveReaders = new Dictionary<Type, MethodInfo>
        {
            { typeof(byte), typeof(Parsing.TrackingReader).GetMethod(nameof(Parsing.TrackingReader.ReadByteAt)) },
            { typeof(short), typeof(Parsing.TrackingReader).GetMethod(nameof(Parsing.TrackingReader.ReadInt16At)) },
            { typeof(ushort), typeof(Parsing.TrackingReader).GetMethod(nameof(Parsing.TrackingReader.ReadUInt16At)) },
            { typeof(int), MI.TrackingReader.ReadInt32At},
            { typeof(uint), typeof(Parsing.TrackingReader).GetMethod(nameof(Parsing.TrackingReader.ReadUInt32At)) },
            { typeof(float), typeof(Parsing.TrackingReader).GetMethod(nameof(Parsing.TrackingReader.ReadFloatAt)) },
            { typeof(string), typeof(Parsing.TrackingReader).GetMethod(nameof(Parsing.TrackingReader.ReadStringFrom)) },
            { typeof(Vector2), typeof(Parsing.TrackingReader).GetMethod(nameof(Parsing.TrackingReader.ReadVec2At)) },
            { typeof(Vector3), typeof(Parsing.TrackingReader).GetMethod(nameof(Parsing.TrackingReader.ReadVec3At)) },
            { typeof(Vector4), typeof(Parsing.TrackingReader).GetMethod(nameof(Parsing.TrackingReader.ReadVec4At)) },
            { typeof(Matrix4x4), typeof(Parsing.TrackingReader).GetMethod(nameof(Parsing.TrackingReader.ReadMatrix4x4At)) },

            { typeof(TagRef), typeof(Parsing.TrackingReader).GetMethod(nameof(Parsing.TrackingReader.ReadTagRefAt)) },
            { typeof(TagRef<>), typeof(Parsing.TrackingReader).GetMethod(nameof(Parsing.TrackingReader.ReadTagRefAt)) },
            { typeof(InternedString), typeof(Parsing.TrackingReader).GetMethod(nameof(Parsing.TrackingReader.ReadInternedStringAt)) },
        };

        public static Dictionary<Type, int> PrimitiveSizes = new Dictionary<Type, int>
        {
            { typeof(byte), 1 },
            { typeof(short), 2 },
            { typeof(ushort), 2 },
            { typeof(int), 4},
            { typeof(uint), 4 },
            { typeof(float), 4 },
        };

        public static class Runtime
        {
            public static MethodInfo GetUninitializedObject = typeof(FormatterServices)
                .GetMethod(nameof(FormatterServices.GetUninitializedObject));
        }

        public static class SystemType
        {
            public static MethodInfo GetTypeFromHandle = typeof(Type)
                .GetMethod(nameof(Type.GetTypeFromHandle));
        }

        public static class TrackingReader
        {
            public static MethodInfo ReadInt32At = typeof(Parsing.TrackingReader)
                .GetMethod(nameof(Parsing.TrackingReader.ReadInt32At));

            public static MethodInfo ReadMetaCaoAt = typeof(Parsing.TrackingReader)
                .GetMethod(nameof(Parsing.TrackingReader.ReadMetaCaoAt), new[] { typeof(int), typeof(int) });
            
            public static MethodInfo ReadArray = typeof(Parsing.TrackingReader)
                .GetMethod(nameof(Parsing.TrackingReader.ReadArray), new[] { typeof(int), typeof(int) });
        }

        public static class Cao
        {
            public static ConstructorInfo Ctor = typeof(CountAndOffset)
                .GetConstructor(new[] { typeof(int), typeof(IOffset) });

            public static MethodInfo OffsetGetter = typeof(CountAndOffset)
               .GetProperty(nameof(CountAndOffset.Offset)).GetGetMethod();

            public static MethodInfo CountGetter = typeof(CountAndOffset)
                .GetProperty(nameof(CountAndOffset.Count)).GetGetMethod();

            public static MethodInfo IntDictAddMethod = typeof(Dictionary<int, CountAndOffset>)
                .GetMethod("Add", new[] { typeof(int), typeof(CountAndOffset) });

            public static MethodInfo IntDictLookup = typeof(Dictionary<int, CountAndOffset>)
                .GetMethod("get_Item", new[] { typeof(int) });
        }

        public static class Offset
        {
            public static ConstructorInfo TagInternalOffsetCtor = typeof(TagInternalOffset)
                .GetConstructor(new[] { typeof(int), typeof(int) });

            public static MethodInfo IOffsetValue = typeof(IOffset)
                .GetProperty(nameof(IOffset.Value)).GetGetMethod();
        }

        public static class Tag
        {
            public static MethodInfo NameGetter = typeof(BaseTag)
               .GetProperty(nameof(BaseTag.Name)).GetGetMethod();
        }

        public static class String
        {
            public static MethodInfo Concat = typeof(string)
                .GetMethod(nameof(string.Concat), new[] { typeof(string), typeof(string) });
        }

        public static class Helpers
        {
            public static MethodInfo ConsoleWriteLine = typeof(Console)
                .GetMethod(nameof(Console.WriteLine), new[] { typeof(string) });
        }
    }
}
