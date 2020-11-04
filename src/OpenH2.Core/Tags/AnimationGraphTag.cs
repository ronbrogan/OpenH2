using OpenH2.Core.Extensions;
using OpenH2.Core.Maps;
using OpenH2.Core.Parsing;
using OpenH2.Core.Tags.Layout;
using OpenH2.Serialization.Layout;
using System;
using System.Numerics;

namespace OpenH2.Core.Tags
{
    [TagLabel(TagName.jmad)]
    public class AnimationGraphTag : BaseTag
    {
        public AnimationGraphTag(uint id) : base(id)
        {
        }

        [PrimitiveValue(4)]
        public TagRef<AnimationGraphTag> Unknown { get; set; }

        [ReferenceArray(12)]
        public Obj12[] Obj12s { get; set; }

        [ReferenceArray(44)]
        public PositionTrack[] Tracks { get; set; }

        [ReferenceArray(52)]
        public Obj1556[] Obj1556s { get; set; }

        [ReferenceArray(84)]
        public Obj1656[] Obj1656s { get; set; }

        public override void PopulateExternalData(H2MapReader reader)
        {
            return;

            // This only works for positions, breaks for first person animations, for example
            foreach (var track in Tracks)
            {
                var frames = track.Values[8];

                Span<byte> data = track.Data;

                var rotStart = 64;
                var posStart = 64 + data.ReadInt32At(52);

                var frameData = new Matrix4x4[frames];

                for (int i = 0; i < frames; i++)
                {
                    var quatOffset = i * 8;
                    var thisQuat = rotStart + quatOffset;

                    var quat = new Quaternion(
                        Decompress(data.ReadInt16At(thisQuat + 0)),
                        Decompress(data.ReadInt16At(thisQuat + 2)),
                        Decompress(data.ReadInt16At(thisQuat + 4)),
                        Decompress(data.ReadInt16At(thisQuat + 6))
                    );

                    var posOffset = i * 12;
                    var thisPos = posStart + posOffset;
                    var pos = new Vector3(
                        data.ReadFloatAt(thisPos + 0),
                        data.ReadFloatAt(thisPos + 4),
                        data.ReadFloatAt(thisPos + 8)
                    );

                    frameData[i] = Matrix4x4.Multiply(
                        Matrix4x4.CreateFromQuaternion(quat), 
                        Matrix4x4.CreateTranslation(pos));
                }
            }

            float Decompress(short v) => v / 32768.0f;
        }

        [FixedLength(32)]
        public class Obj12
        {
            [InternedString(0)]
            public string Description { get; set; }

            [PrimitiveValue(4)]
            public ushort ValueA { get; set; }

            [PrimitiveValue(6)]
            public ushort ValueB { get; set; }

            [PrimitiveValue(8)]
            public ushort ValueC { get; set; }

            [PrimitiveValue(10)]
            public ushort ValueD { get; set; }

            [PrimitiveArray(12, 5)]
            public float[] Params { get; set; }
        }

        [FixedLength(96)]
        public class PositionTrack
        {
            [InternedString(0)]
            public string Description { get; set; }


            [PrimitiveArray(4, 18)]
            public ushort[] Values { get; set; }

            public ushort ValueA { get; set; }

            [ReferenceArray(40)]
            public byte[] Data { get; set; }

            public Matrix4x4[] Frames { get; set; }
        }

        [FixedLength(20)]
        public class Obj1556
        {
            [PrimitiveValue(0)]
            public ushort IndexA { get; set; }

            [PrimitiveValue(2)]
            public ushort IndexB { get; set; }

            [PrimitiveArray(4,2)]
            public uint[] Next { get; set; }

            [ReferenceArray(12)]
            public Obj1556Val[] Value { get; set; }
        }


        [FixedLength(8)]
        public class Obj1556Val
        {
            [PrimitiveValue(0)]
            public ushort ValueA { get; set; }

            [PrimitiveValue(2)]
            public ushort ValueB { get; set; }

            [PrimitiveValue(4)]
            public ushort ValueC { get; set; }

            [PrimitiveValue(6)]
            public ushort ValueD { get; set; }
        }

        [FixedLength(8)]
        public class Obj1656
        {
            [PrimitiveValue(0)]
            public ushort ValueA { get; set; }

            [PrimitiveValue(2)]
            public ushort ValueB { get; set; }

            [PrimitiveValue(4)]
            public ushort ValueC { get; set; }

            [PrimitiveValue(6)]
            public ushort ValueD { get; set; }
        }
    }
}
