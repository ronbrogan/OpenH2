using OpenH2.Core.Maps;
using OpenH2.Core.Tags.Layout;
using OpenBlam.Serialization.Layout;
using System.Numerics;

namespace OpenH2.Core.Tags
{
    [TagLabel(TagName.shad)]
    public class ShaderTag : BaseTag
    {
        public override string Name { get; set; }

        public ShaderTag(uint id) : base(id)
        {
        }

        [StringValue(0, 4)]
        public string StemTag { get; set; }

        [PrimitiveValue(4)]
        public TagRef<ShaderTemplateTag> ShaderTemplate { get; set; }

        [InternedString(8)]
        public string MaterialName { get; set; }

        [ReferenceArray(12)]
        public LegacyBitmapInfo[] BitmapInfos { get; set; }

        [ReferenceArray(32)]
        public ShaderTemplateArguments[] Arguments { get; set; }

        // Predicted resources?
        //[ReferenceArray(44)]
        //public BitmapReferenceSetting[] BitmapReferenceSettings { get; set; }

        [FixedLength(80)]
        public class LegacyBitmapInfo
        {
            [PrimitiveValue(4)]
            public TagRef<BitmapTag> DiffuseBitmap { get; set; }

            [PrimitiveValue(12)]
            public TagRef<BitmapTag> EmissiveBitmap { get; set; }

            [PrimitiveValue(16)]
            public float Param1 { get; set; }

            [PrimitiveValue(20)]
            public float Param2 { get; set; }

            [PrimitiveValue(24)]
            public float Param3 { get; set; }

            [PrimitiveValue(28)]
            public float Param4 { get; set; }

            [PrimitiveValue(48)]
            public TagRef<BitmapTag> AlphaBitmap { get; set; }
        }

        [FixedLength(124)]
        public class ShaderTemplateArguments
        {
            [PrimitiveValue(0)]
            public TagRef<ShaderTemplateTag> ShaderTemplate { get; set; }

            [ReferenceArray(4)]
            public ShaderMap[] BitmapArguments { get; set; }

            [ReferenceArray(12)]
            public BitmapParameter2[] BitmapParamter2s { get; set; }

            [ReferenceArray(20)]
            public Vector4[] ShaderInputs { get; set; }

            [ReferenceArray(28)]
            public BitmapParameter4[] BitmapParamter4s { get; set; }

            [ReferenceArray(36)]
            public Obj36[] Obj36s { get; set; }

            [ReferenceArray(44)]
            public Obj44[] Obj44s { get; set; }

            // Obj52 is ~10 bytes long and only observed as 0s thus far

            [ReferenceArray(60)]
            public FunctionArgumentData[] FunctionArguments { get; set; }

            [ReferenceArray(92)]
            public WellKnownMapProperty[] WellKnownMapProperties { get; set; }

            [ReferenceArray(100)]
            public Vector3[] ConstantColorArguments { get; set; }

            [ReferenceArray(108)]
            public float[] ConstantValueArguments { get; set; }

            [FixedLength(12)]
            public class ShaderMap
            {
                [PrimitiveValue(0)]
                public TagRef<BitmapTag> Bitmap { get; set; }

                [PrimitiveValue(4)]
                public Vector2 Something { get; set; }

                [PrimitiveValue(8)]
                public byte A { get; set; }

                [PrimitiveValue(9)]
                public byte B { get; set; }

                [PrimitiveValue(10)]
                public byte C { get; set; }

                [PrimitiveValue(11)]
                public byte D { get; set; }
            }

            [FixedLength(4)]
            public class BitmapParameter2
            {
                [PrimitiveValue(0)]
                public byte A { get; set; }

                [PrimitiveValue(1)]
                public byte B { get; set; }

                [PrimitiveValue(2)]
                public byte C { get; set; }

                [PrimitiveValue(3)]
                public byte D { get; set; }
            }

            [FixedLength(6)]
            public class BitmapParameter4
            {
                [PrimitiveValue(0)]
                public ushort ValueA { get; set; }

                [PrimitiveValue(2)]
                public ushort ValueB { get; set; }

                [PrimitiveValue(4)]
                public byte ValueC { get; set; }

                [PrimitiveValue(5)]
                public byte ValueD { get; set; }
            }

            [FixedLength(2)]
            public class Obj36
            {
                [PrimitiveValue(0)]
                public byte Obj44Index { get; set; }

                [PrimitiveValue(1)]
                public byte B { get; set; }
            }

            [FixedLength(2)]
            public class Obj44
            {
                [PrimitiveValue(0)]
                public byte A { get; set; }

                [PrimitiveValue(1)]
                public byte B { get; set; }
            }

            [FixedLength(20)]
            public class FunctionArgumentData
            {
                [PrimitiveValue(0)]
                public Vector3 Values { get; set; }

                [ReferenceArray(12)]
                public byte[] Data { get; set; }
            }

            // These seem to line up with the 'Properties' in the shader template
            // 0 Diffuse
            // 1 Emissive
            // 2 LightmapAlpha
            // 3 LightmapTranslucent
            // 4 ActiveCamo
            [FixedLength(4)]
            public class WellKnownMapProperty
            {
                [PrimitiveValue(0)]
                public ushort MapIndex { get; set; }

                [PrimitiveValue(2)]
                public ushort B { get; set; }
            }
        }



        // Predicted resources?
        //[FixedLength(8)]
        //public class BitmapReferenceSetting
        //{
        //    [PrimitiveValue(0)]
        //    public short ValueA { get; set; }
        //
        //    [PrimitiveValue(2)]
        //    public short ValueB { get; set; }
        //
        //    [PrimitiveValue(4)]
        //    public uint BitmapId { get; set; }
        //}
    }
}
