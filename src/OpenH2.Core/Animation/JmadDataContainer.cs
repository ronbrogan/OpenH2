using System;
using System.Diagnostics;
using System.Numerics;
using OpenBlam.Core.Extensions;

namespace OpenH2.Core.Animation
{
    public enum JmadDataType
    {
        /// <summary>
        /// Quaternions (compressed into shorts) and Vector3 are present
        /// Typically (always?) comes as separate section before animation data
        /// </summary>
        Flat = 1,

        /// <summary>
        /// Quaternions (compressed into shorts) and Vector3 are present in the 
        /// animation data as values in blocks of FrameCount items
        /// Has 'Flat' preamble
        /// </summary>
        Normal = 3,

        /// <summary>
        /// Quaternions (compressed into shorts) and Vector3 are present in the 
        /// animation data as values. There exists a set of FrameMapping data
        /// that maps each value to frame index
        /// Has 'Flat' preamble
        /// </summary>
        Four = 4,

        /// <summary>
        /// Quaternions (compressed into shorts) and Vector3 are present in the 
        /// animation data as values. There exists a set of FrameMapping data
        /// that maps each value to frame index.
        /// This format uses shorts for the FrameMapping, as this is commonly used for
        /// very long animations.
        /// Has 'Flat' preamble
        /// </summary>
        Five = 5,

        /// <summary>
        /// Quaternions (compressed into shorts) and Vector3 are present in the 
        /// animation data as values. There exists a set of FrameMapping data
        /// that maps each value to frame index
        /// Has 'Flat' preamble
        /// </summary>
        Six = 6,

        /// <summary>
        /// Quaternions (compressed into shorts) and Vector3 are present in the 
        /// animation data as values. There exists a set of FrameMapping data
        /// that maps each value to frame index.
        /// This format uses shorts for the FrameMapping, as this is commonly used for
        /// very long animations.
        /// Has 'Flat' preamble
        /// </summary>
        Seven = 7,

        /// <summary>
        /// Quaternions and Vector3 are present in the animation data as single-precision
        /// floating point values in blocks of FrameCount items
        /// No pre-header data
        /// </summary>
        UncompressedNormal = 8
    }

    public ref struct JmadDataContainer
    {
        private delegate Quaternion OrientationReader(Span<byte> data, int offset);
        private delegate Vector3 TranslationReader(Span<byte> data, int offset);

        public readonly Span<byte> Data;

        public JmadDataType Type;
        public int OrientationCount;
        public int TranslationCount;
        public int ScaleCount;

        public int OrientationVariableSizeBlockOffset;
        public int TranslationVariableSizeBlockOffset;
        public int ScaleVariableSizeBlockOffset;
        private bool ReversedVariableSizeBlocks;

        public int OrientationFrameMapping;
        public int TranslationFrameMapping;
        public int ScaleFrameMapping;
        public bool ShortsForFrameMapping;

        public int OrientationOffset;
        public int TranslationOffset;
        public int ScaleOffset;

        public int OrientationSize;
        public int TranslationSize;
        public int ScaleSize;

        private OrientationReader orientationReader;
        private int orientationElementSize;
        private TranslationReader translationReader;
        private int translationElementSize;

        public static JmadDataContainer Create(Span<byte> data)
        {
            var jmad = new JmadDataContainer(data);

            switch (jmad.Type)
            {
                case JmadDataType.Flat:
                    jmad.SetComponentOffsets(32, data.ReadInt32At(12), data.ReadInt32At(16));
                    break;
                case JmadDataType.Normal:
                    jmad.SetComponentOffsets(32, data.ReadInt32At(12), data.ReadInt32At(16));
                    jmad.SetComponentSizes(data.ReadInt32At(20), data.ReadInt32At(24), data.ReadInt32At(28));
                    break;
                case JmadDataType.Four:
                    jmad.SetVariableComponentSizeBlocks(48, data.ReadInt32At(12), data.ReadInt32At(16));
                    jmad.SetFrameMappingOffsets(data.ReadInt32At(20), data.ReadInt32At(24), data.ReadInt32At(28));
                    jmad.SetComponentOffsets(data.ReadInt32At(32), data.ReadInt32At(36), data.ReadInt32At(40));
                    break;
                case JmadDataType.Five:
                    jmad.SetVariableComponentSizeBlocks(48, data.ReadInt32At(12), data.ReadInt32At(16));
                    jmad.SetFrameMappingOffsets(data.ReadInt32At(20), data.ReadInt32At(24), data.ReadInt32At(28), usesShorts: true);
                    jmad.SetComponentOffsets(data.ReadInt32At(32), data.ReadInt32At(36), data.ReadInt32At(40));
                    break;
                case JmadDataType.Six:
                    jmad.SetVariableComponentSizeBlocks(48, data.ReadInt32At(12), data.ReadInt32At(16), reversedBlocks: true);
                    jmad.SetFrameMappingOffsets(data.ReadInt32At(20), data.ReadInt32At(24), data.ReadInt32At(28));
                    jmad.SetComponentOffsets(data.ReadInt32At(32), data.ReadInt32At(36), data.ReadInt32At(40));
                    break;
                case JmadDataType.Seven:
                    jmad.SetVariableComponentSizeBlocks(48, data.ReadInt32At(12), data.ReadInt32At(16), reversedBlocks: true);
                    jmad.SetFrameMappingOffsets(data.ReadInt32At(20), data.ReadInt32At(24), data.ReadInt32At(28), usesShorts: true);
                    jmad.SetComponentOffsets(data.ReadInt32At(32), data.ReadInt32At(36), data.ReadInt32At(40));
                    break;
                case JmadDataType.UncompressedNormal:
                    jmad.SetComponentOffsets(32, data.ReadInt32At(12), data.ReadInt32At(16));
                    jmad.SetComponentSizes(data.ReadInt32At(20), data.ReadInt32At(24), data.ReadInt32At(28));
                    break;
                default:
                    Debug.Fail("Unsupported data type");
                    break;
            }

            return jmad;
        }

        private JmadDataContainer(Span<byte> data)
        {
            this.Data = data;
            this.Type = (JmadDataType)data[0];
            this.OrientationCount = data[1];
            this.TranslationCount = data[2];
            this.ScaleCount = data[3];

            this.OrientationOffset = -1;
            this.TranslationOffset = -1;
            this.ScaleOffset = -1;
            this.OrientationSize = -1;
            this.TranslationSize = -1;
            this.ScaleSize = -1;

            this.OrientationVariableSizeBlockOffset = -1;
            this.TranslationVariableSizeBlockOffset = -1;
            this.ScaleVariableSizeBlockOffset = -1;
            this.ReversedVariableSizeBlocks = false;

            this.OrientationFrameMapping = -1;
            this.TranslationFrameMapping = -1;
            this.ScaleFrameMapping = -1;
            this.ShortsForFrameMapping = false;

            this.orientationReader = ReadCompressedQuat;
            this.orientationElementSize = 8;

            if (this.Type == JmadDataType.UncompressedNormal)
            {
                this.orientationReader = ReadQuat;
                this.orientationElementSize = 16;
            }

            this.translationReader = ReadVec3;
            this.translationElementSize = 12;
        }

        public void SetComponentOffsets(int orientation, int translation, int scale)
        {
            this.OrientationOffset = orientation;
            this.TranslationOffset = translation;
            this.ScaleOffset = scale;
        }

        public void SetComponentSizes(int orientation, int translation, int scale)
        {
            this.OrientationSize = orientation;
            this.TranslationSize = translation;
            this.ScaleSize = scale;
        }

        public void SetFrameMappingOffsets(int orientation, int translation, int scale, bool usesShorts = false)
        {
            this.OrientationFrameMapping = orientation;
            this.TranslationFrameMapping = translation;
            this.ScaleFrameMapping = scale;
            this.ShortsForFrameMapping = usesShorts;
        }

        /// <summary>
        /// These items will have [Component count] 4 byte values, first byte is element count, 
        /// next ushort is offset from section basis
        /// </summary>
        public void SetVariableComponentSizeBlocks(int orientation, int translation, int scale, bool reversedBlocks = false)
        {
            this.OrientationVariableSizeBlockOffset = orientation;
            this.TranslationVariableSizeBlockOffset = translation;
            this.ScaleVariableSizeBlockOffset = scale;
            this.ReversedVariableSizeBlocks = reversedBlocks;
        }

        public int TotalLength()
        {
            return ScaleOffset + ScaleCount * sizeof(float);
        }

        public Quaternion ReadOrientation(Span<byte> data, int frameIndex, int boneIndex)
        {
            var quatOffset = -1;

            if (boneIndex >= this.OrientationCount)
            {
                return Quaternion.Identity;
            }
            else if (this.Type == JmadDataType.Normal || this.Type == JmadDataType.UncompressedNormal)
            {
                Debug.Assert(this.OrientationSize > 0, "Orientation Size must be set for Normal types");

                if (boneIndex < this.OrientationCount)
                {
                    var boneDataOffset = this.OrientationOffset + (boneIndex * this.OrientationSize);

                    quatOffset = boneDataOffset + (frameIndex * this.orientationElementSize);
                }
            }
            else
            {
                var (mappingOffset, mappingLength) = GetMappingData(ComponentType.Orientation, boneIndex);

                var absoluteMappingOffset = this.OrientationFrameMapping + mappingOffset;
                var mappingElementSize = (this.ShortsForFrameMapping ? 2 : 1);
                var dataOffset = this.OrientationOffset + ((mappingOffset / mappingElementSize) * this.orientationElementSize);

                for (int i = 0; i < mappingLength; i++)
                {
                    var internalOffset = i * mappingElementSize;

                    int frameMap = this.ShortsForFrameMapping
                        ? this.Data.ReadUInt16At(absoluteMappingOffset + internalOffset)
                        : this.Data[absoluteMappingOffset + internalOffset];

                    if(frameMap == frameIndex)
                    {
                        quatOffset = dataOffset + (i * this.orientationElementSize);
                        break;
                    }
                }
            }

            if(quatOffset == -1)
            {
                return Quaternion.Identity;
            }

            var quat = this.orientationReader(data, quatOffset);

            Debug.Assert(Math.Abs(quat.Length() - 1) < 0.001f, $"Quaternions must be unit length", "Quat at: {0}, {1}", frameIndex, boneIndex);

            return quat;
        }

        public Vector3 ReadTranslation(Span<byte> data, int frameIndex, int boneIndex)
        {
            var vecOffset = -1;

            if (boneIndex >= this.TranslationCount)
            {
                return Vector3.Zero;
            }
            else if (this.Type == JmadDataType.Normal || this.Type == JmadDataType.UncompressedNormal)
            {
                Debug.Assert(this.TranslationSize > 0, "Translation Size must be set for Normal types");

                if (boneIndex < this.TranslationCount)
                {
                    var boneDataOffset = this.TranslationOffset + (boneIndex * this.TranslationSize);

                    vecOffset = boneDataOffset + (frameIndex * this.translationElementSize);
                }
            }
            else
            {
                var (mappingOffset, mappingLength) = GetMappingData(ComponentType.Translation, boneIndex);

                var absoluteMappingOffset = this.TranslationFrameMapping + mappingOffset;
                var mappingElementSize = (this.ShortsForFrameMapping ? 2 : 1);
                var dataOffset = this.TranslationOffset + ((mappingOffset / mappingElementSize) * this.translationElementSize);

                for (int i = 0; i < mappingLength; i++)
                {
                    var internalOffset = i * mappingElementSize;

                    int frameMap = this.ShortsForFrameMapping
                        ? this.Data.ReadUInt16At(absoluteMappingOffset + internalOffset)
                        : this.Data[absoluteMappingOffset + internalOffset];

                    if (frameMap == frameIndex)
                    {
                        vecOffset = dataOffset + (i * this.translationElementSize);
                        break;
                    }
                }
            }

            if (vecOffset == -1)
            {
                return Vector3.Zero;
            }

            return this.translationReader(data, vecOffset);
        }

        private (int offset, byte length) GetMappingData(ComponentType type, int boneIndex)
        {
            var (componentBase, componentCount, blockBase) = type switch
            {
                ComponentType.Orientation => (this.OrientationOffset, this.OrientationCount, this.OrientationVariableSizeBlockOffset),
                ComponentType.Translation => (this.TranslationOffset, this.TranslationCount, this.TranslationVariableSizeBlockOffset)
            };

            var current = 0;
            Action next = () => current+=4;

            if(this.ReversedVariableSizeBlocks)
            {
                current = 4 * (componentCount-1);
                next = () => current -= 4;
            }

            var mappingOffset = 0;
            byte mappingLength = 0;
            ushort dataOffset = 0;

            for (int i = 0; i < boneIndex+1; i++, next())
            {
                var itemBase = blockBase + current;

                mappingOffset += mappingLength;

                mappingLength = this.Data[itemBase];

                // This isn't always correct
                //dataOffset = this.Data.ReadUInt16At(itemBase + 1);
            }

            return (mappingOffset, mappingLength);
        }

        private static Quaternion ReadCompressedQuat(Span<byte> data, int offset)
        {
            var val = new Quaternion(
                Decompress(data.ReadInt16At(offset + 0)),
                Decompress(data.ReadInt16At(offset + 2)),
                Decompress(data.ReadInt16At(offset + 4)),
                Decompress(data.ReadInt16At(offset + 6))
            );

            return val;

            float Decompress(short v) => v / 32768.0f;
        }

        private static Quaternion ReadQuat(Span<byte> data, int offset)
        {
            return data.ReadQuaternionAt(offset);
        }

        private static Vector3 ReadVec3(Span<byte> data, int offset)
        {
            return data.ReadVec3At(offset);
        }

        private enum ComponentType
        {
            Orientation,
            Translation,
            Scale
        }
    }
}
