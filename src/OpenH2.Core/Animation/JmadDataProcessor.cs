using OpenBlam.Core.Extensions;
using OpenH2.Core.Extensions;
using System;
using System.Diagnostics;
using System.Numerics;

namespace OpenH2.Core.Animation
{
    public enum JmadDataType
    {
        Flat = 1,
        Three = 3,
        Four = 4,
        Five = 5,
        Six = 6,
        Seven = 7,
        Eight = 8
    }

    public struct JmadDataHeader
    {
        public JmadDataType Type;
        public int OrientationCount;
        public int TranslationCount;
        public int ScaleCount;

        public int OrientationOffset;
        public int TranslationOffset;
        public int ScaleOffset;

        public int TotalLength()
        {
            return ScaleOffset + ScaleCount * sizeof(float);
        }
    }

    public class JmadDataProcessor : IAnimationProcessor
    {
        private delegate int OrientationFrameSetCount(Span<byte> data);
        private delegate int TranslationFrameSetCount(Span<byte> data);

        private JmadDataProcessor() { }
        public static JmadDataProcessor GetProcessor()
        {
            var processor = new JmadDataProcessor();

            return processor;
        }

        public Animation GetAnimation(int frames, int bones, Span<byte> allData)
        {
            var data = allData.Slice(ReadHeader(allData).TotalLength());
            var header = ReadHeader(data);


            var setCount = Math.Max(header.OrientationCount, Math.Max(header.TranslationCount, header.ScaleCount));
            Debug.Assert(setCount <= bones);

            var frameData = new AnimationFrame[setCount, frames];

            for(var i = 0; i < header.OrientationCount; i++)
            {
                for (int j = 0; j < frames; j++)
                {
                    var quatOffset = j * 8;
                    var thisQuat = header.OrientationOffset + i * 8 * frames + quatOffset;

                    var quat = new Quaternion(
                        Decompress(data.ReadInt16At(thisQuat + 0)),
                        Decompress(data.ReadInt16At(thisQuat + 2)),
                        Decompress(data.ReadInt16At(thisQuat + 4)),
                        Decompress(data.ReadInt16At(thisQuat + 6))
                    );

                    var length = quat.Length();

                    Debug.Assert(Math.Abs(length - 1) < 0.001f, $"Quaternions must be unit length", "Quat at: {0}, {1}", i, j);

                    frameData[i, j].Orientation = quat;
                }
            }

            for (var i = 0; i < header.TranslationCount; i++)
            {
                for (int j = 0; j < frames; j++)
                {
                    var posOffset = j * 12;
                    var thisPos = header.TranslationOffset + i * 12 * frames + posOffset;

                    var pos = new Vector3(
                        data.ReadFloatAt(thisPos + 0),
                        data.ReadFloatAt(thisPos + 4),
                        data.ReadFloatAt(thisPos + 8)
                    );

                    frameData[i, j].Translation = pos;
                }
            }

            return new Animation() { Frames = frameData };
        }

        private static float Decompress(short v) => v / 32768.0f;


        public JmadDataHeader ReadHeader(Span<byte> data)
        {
            var header = new JmadDataHeader
            {
                Type = (JmadDataType)data[0],
                OrientationCount = data[1],
                TranslationCount = data[2],
                ScaleCount = data[3]
            };

            switch (header.Type)
            {
                case JmadDataType.Flat:
                    header.OrientationOffset = 32;
                    header.TranslationOffset = data.ReadInt32At(12);
                    header.ScaleOffset = data.ReadInt32At(16);
                    break;
                case JmadDataType.Three:
                    header.OrientationOffset = 32;
                    header.TranslationOffset = data.ReadInt32At(12);
                    header.ScaleOffset = data.ReadInt32At(16);
                    break;
                case JmadDataType.Six:
                    header.OrientationOffset = data.ReadInt32At(32);
                    header.TranslationOffset = data.ReadInt32At(36);
                    header.ScaleOffset = data.ReadInt32At(40);
                    break;
                default:
                    //Debug.Fail("Unsupported data type");
                    break;
            }

            return header;
        }


    }
}
