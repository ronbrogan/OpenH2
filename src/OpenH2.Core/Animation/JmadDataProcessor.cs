using OpenBlam.Core.Extensions;
using OpenH2.Core.Extensions;
using System;
using System.Diagnostics;
using System.Numerics;

namespace OpenH2.Core.Animation
{
    public class JmadDataProcessor : IAnimationProcessor
    {
        private delegate int OrientationFrameSetCount(Span<byte> data);
        private delegate int TranslationFrameSetCount(Span<byte> data);

        private JmadDataProcessor() { }
        public static IAnimationProcessor GetProcessor()
        {
            var processor = new JmadDataProcessor();

            return processor;
        }

        public Animation GetAnimation(int frames, int bones, Span<byte> data)
        {
            var orientationSets = GetOrientationBoneCount(data);
            var translationSets = GetTranslationBoneCount(data);

            var setCount = Math.Max(orientationSets, translationSets);
            Debug.Assert(setCount <= bones);

            var dataStart = GetFrameDataStart(data);

            var frameData = new AnimationFrame[setCount, frames];

            var orientStart = dataStart;
            for(var i = 0; i < orientationSets; i++)
            {
                for (int j = 0; j < frames; j++)
                {
                    var quatOffset = j * 8;
                    var thisQuat = orientStart + i * 8 * frames + quatOffset;

                    var quat = new Quaternion(
                        Decompress(data.ReadInt16At(thisQuat + 0)),
                        Decompress(data.ReadInt16At(thisQuat + 2)),
                        Decompress(data.ReadInt16At(thisQuat + 4)),
                        Decompress(data.ReadInt16At(thisQuat + 6))
                    );

                    Debug.Assert(Math.Abs(quat.Length() - 1) < 0.001f, $"Quaternions must be unit length", "Quat at: {0}, {1}", i, j);

                    frameData[i, j].Orientation = quat;
                }
            }

            var translateStart = dataStart + 8 * frames * orientationSets;
            for (var i = 0; i < translationSets; i++)
            {
                for (int j = 0; j < frames; j++)
                {
                    var posOffset = j * 12;
                    var thisPos = translateStart + i * 12 * frames + posOffset;

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

        private int GetInfoOffset(Span<byte> data)
        {
            // Guessing that 0x3 indicates that there's a float in front of info header
            var threeVal = data.ReadByteAt(3);

            return data.ReadInt32At(16) + (threeVal == 1 ? 4 : 0);
        }

        private int GetFrameDataStart(Span<byte> data)
        {
            var infoOffset = GetInfoOffset(data);
            var sizeCount = data.ReadByteAt(infoOffset);

            // 4 byte head, 2 floats, 2 static sizes, sizeCount sizes
            return infoOffset + 4 + 8 + 8 + sizeCount * 4;
        }

        private int GetOrientationBoneCount(Span<byte> data)
        {
            var infoOffset = GetInfoOffset(data);
            return data.ReadByteAt(infoOffset + 1);
        }

        private int GetTranslationBoneCount(Span<byte> data)
        {
            var infoOffset = GetInfoOffset(data);
            return data.ReadByteAt(infoOffset + 2);
        }

        private static float Decompress(short v) => v / 32768.0f;
    }
}
