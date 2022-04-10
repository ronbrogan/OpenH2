using OpenBlam.Core.Extensions;
using OpenH2.Core.Extensions;
using OpenH2.Core.Maps;
using OpenH2.Core.Tags.Layout;
using OpenBlam.Serialization.Layout;
using System;
using System.Numerics;
using OpenBlam.Core.MapLoading;

namespace OpenH2.Core.Tags
{
    [Flags]
    public enum AnimationInheritanceFlags : byte
    {
        InheritRootTransScaleOnly = 1,
        InheritForUseOnPlayer = 2
    }

    [Flags]
    public enum AnimationFlags : byte
    {
        PreparedForCache = 1,
        Unused = 2,
        ImportedWithCodecCompressors = 4,
        UnusedSmellyFlag = 8,
        WrittenToCache = 16,
        AnimationDataRecorded = 32
    }

    [Flags]
    public enum AnimationInternalFlags : byte
    {
        Unused0 = 1,
        WorldRelative = 2,
        Unused1 = 4,
        Unused2 = 8,
        Unused3 = 16,
        CompressionDisabled = 32,
        OldProductionChecksum = 64,
        ValidProductionChecksum = 128,
    }

    [Flags]
    public enum AnimationProductionFlags : byte
    {
        DoNotMonitorChanges = 1,
        VerifySoundEvents = 2,
        DoNotInheritForPlayerGraphs = 4,
    }

    [Flags]
    public enum AnimationPlaybackFlags : ushort
    {
        DisableInterpolationIN = 1,
        DisableInterpolationOUT = 2,
        DisableModeIK = 4,
        DisableWeaponIK = 8,
        DisableWeaponAim1P = 16,
        DisableLookScreen = 32,
        DisableTransitionAdjustment = 64
    }


    [TagLabel(TagName.jmad)]
    public class AnimationGraphTag : BaseTag
    {
        public AnimationGraphTag(uint id) : base(id)
        {
        }

        [PrimitiveValue(4)]
        public TagRef<AnimationGraphTag> ParentAnimation { get; set; }

        [PrimitiveValue(8)]
        public AnimationInheritanceFlags InheritanceFlags { get; set; }

        [PrimitiveValue(9)]
        public AnimationFlags PrivateFlags { get; set; }

        [PrimitiveValue(10)]
        public short AnimationCodecPack { get; set; }

        [ReferenceArray(12)]
        public SkeletonNode[] Nodes { get; set; }

        [ReferenceArray(20)]
        public AnimationSoundReference[] Sounds { get; set; }

        // 28 is effect refrences

        // 36 is blend screens

        [ReferenceArray(44)]
        public Animation[] Animations { get; set; }

        [ReferenceArray(52)]
        public Obj1556[] Obj1556s { get; set; }

        [ReferenceArray(84)]
        public Obj1656[] Obj1656s { get; set; }

        public override void PopulateExternalData(MapStream reader)
        {
            return;

            // This only works for positions, breaks for first person animations, for example
            foreach (var track in Animations)
            {
                var frames = track.FrameCount;

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
        public class SkeletonNode
        {
            [InternedString(0)]
            public string Description { get; set; }

            [PrimitiveValue(4)]
            public ushort Sibling { get; set; }

            [PrimitiveValue(6)]
            public ushort Child { get; set; }

            [PrimitiveValue(8)]
            public ushort Parent { get; set; }

            [PrimitiveValue(10)]
            public ushort Flags { get; set; }

            [PrimitiveArray(12, 5)]
            public float[] Params { get; set; }
        }

        [FixedLength(12)]
        public class AnimationSoundReference
        {
            [PrimitiveValue(4)]
            public TagRef<SoundTag> SoundTag { get; set; }

            [PrimitiveValue(8)]
            public uint Param { get; set; }
        }

        [FixedLength(96)]
        public class Animation
        {
            public enum AnimationType : byte
            {
                Base = 0,
                Overlay = 1,
                Replacement = 2
            }

            public enum AnimationFrameInfo : byte
            {
                None = 0,
                DxDy = 1,
                DxDyDyaw = 2,
                DxDyDzDyaw = 3,
            }

            

            [InternedString(0)]
            public string Name { get; set; }

            [PrimitiveValue(4)]
            public int NodeListChecksum { get; set; }

            [PrimitiveValue(8)]
            public int ProductionChecksum { get; set; }

            [PrimitiveValue(12)]
            public int ImportChecksum { get; set; }

            [PrimitiveValue(16)]
            public AnimationType Type { get; set; }

            [PrimitiveValue(17)]
            public AnimationFrameInfo FrameInfo { get; set; }

            [PrimitiveValue(18)]
            public byte BlendScreenIndex { get; set; }

            [PrimitiveValue(19)]
            public byte NodeCount { get; set; }

            [PrimitiveValue(20)]
            public ushort FrameCount { get; set; }

            [PrimitiveValue(22)]
            public AnimationInternalFlags InternalFlags { get; set; }

            [PrimitiveValue(23)]
            public AnimationProductionFlags ProductionFlags { get; set; }

            [PrimitiveValue(24)]
            public AnimationPlaybackFlags PlaybackFlags { get; set; }

            [PrimitiveValue(26)]
            public byte DesiredCompression { get; set; }

            [PrimitiveValue(27)]
            public byte CurrentCompression { get; set; }

            [PrimitiveValue(28)]
            public float Weight { get; set; }

            [PrimitiveValue(32)]
            public ushort LoopFrame { get; set; }

            [PrimitiveValue(34)]
            public ushort ParentIndex { get; set; }

            [PrimitiveValue(36)]
            public ushort ChildIndex { get; set; }

            [PrimitiveValue(38)]
            public ushort Zero { get; set; }

            [ReferenceArray(40)]
            public byte[] Data { get; set; }

            [PrimitiveValue(48)]
            public byte SizeA { get; set; }

            [PrimitiveValue(49)]
            public byte SizeB { get; set; }

            [PrimitiveValue(50)]
            public ushort SizeC { get; set; }

            [PrimitiveValue(52)]
            public ushort SizeD { get; set; }

            [PrimitiveValue(54)]
            public ushort SizeE { get; set; }

            [PrimitiveValue(60)]
            public ushort FrameDataSize { get; set; }

            [ReferenceArray(64)]
            public FrameEvent[] FrameEvents { get; set; }

            [ReferenceArray(72)]
            public SoundEvent[] SoundEvents { get; set; }

            [FixedLength(4)]
            public class FrameEvent
            {
                [PrimitiveValue(0)]
                public short ValueA { get; set; }

                [PrimitiveValue(2)]
                public short ValueB { get; set; }
            }

            [FixedLength(8)]
            public class SoundEvent
            {
                [PrimitiveValue(0)]
                public short SoundIndex { get; set; }

                [PrimitiveValue(2)]
                public short Frame { get; set; }

                [InternedString(4)]
                public string MarkerName { get; set; }
            }
        }

        [FixedLength(20)]
        public class Obj1556
        {
            [PrimitiveValue(0)]
            public ushort IndexA { get; set; }

            [PrimitiveValue(2)]
            public ushort IndexB { get; set; }

            [PrimitiveArray(4, 2)]
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
