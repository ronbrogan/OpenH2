﻿using OpenH2.Core.Tags.Layout;
using OpenBlam.Serialization.Layout;
using System.Text.Json.Serialization;

namespace OpenH2.Core.Tags
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum EncodingType : byte
    {
        ImaAdpcmMono = 0,
        ImaAdpcmStereo = 1,
        WmaBytestream = 2
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum SampleRate : byte
    {
        hz22k05 = 0,
        hz44k1 = 1,
        hz32k = 2,
        hz48k = 3,
    }

    [TagLabel(TagName.snd)] // snd!
    public class SoundTag : BaseTag
    {
        public override string Name { get; set; }
        public SoundTag(uint id) : base(id)
        {
        }

        [PrimitiveValue(0)]
        public ushort Flags { get; set; }


        [PrimitiveValue(2)]
        public byte Class { get; set; }

        [PrimitiveValue(3)]
        public SampleRate SampleRate { get; set; }

        [PrimitiveValue(4)]
        public EncodingType Encoding { get; set; }

        [PrimitiveValue(5)]
        public byte Format2 { get; set; }

        [PrimitiveValue(6)]
        public ushort Unknown { get; set; }

        [PrimitiveValue(8)]
        public ushort SoundEntryIndex { get; set; }

        [PrimitiveValue(10)]
        public ushort LoopLength { get; set; }

        [PrimitiveValue(12)]
        public ushort UsuallyMaxValue { get; set; }

        [PrimitiveValue(14)]
        public ushort DialogId { get; set; }

        [PrimitiveValue(16)]
        public ushort Duration { get; set; }

        [PrimitiveValue(18)]
        public ushort UsuallyZero { get; set; }
    }
}
