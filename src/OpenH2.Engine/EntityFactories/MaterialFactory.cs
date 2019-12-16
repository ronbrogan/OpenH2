using OpenH2.Core.Enums.Texture;
using OpenH2.Core.Representations;
using OpenH2.Core.Tags;
using OpenH2.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace OpenH2.Engine.EntityFactories
{
    public class MaterialFactory
    {
        private static Dictionary<uint, Action<H2vMap, ShaderTag.ShaderArguments, Material<BitmapTag>>> ShaderMappings = new Dictionary<uint, Action<H2vMap, ShaderTag.ShaderArguments, Material<BitmapTag>>>
        {
            { 3833408024, TwoAddEnvIllum },
            { 3834718764, TexBumpPlasmaOneChannelIllum },
            { 3913548045, PrtSimple },
            { 3884461347, Overlay },
            { 3917480231, TexEnv },
            { 3786024773, TexBump },
            { 3783075608, TexBumpIllum },
            { 4114612757, TexBumpNoAlpha },
            { 3923521399, TexBumpEnvCombined },
            { 4118348365, TexBumpDetailKeep },
            { 4116185644, TexBumpDetailBlend },
            { 4113891850, TexBumpDetailKeepBlend },
            { 3786745680, TransparentPlasmaAlpha },
            { 3788318568, TransparentOneAlphaEnv },
            { 4119855716, TransparentTwoAlphaClouds },
            { 3783524361, SkyTwoAlphaClouds }
        };

        public static void PopulateMaterial(H2vMap map, Material<BitmapTag> mat, ShaderTag shader)
        {
            var args = shader.Arguments[0];

            if(ShaderMappings.TryGetValue(args.ShaderTemplate.Id, out var mapping))
            {
                mapping(map, args, mat);
                return;
            }

            Console.WriteLine($"Using heuristic for shader '{shader.Name}' stem[{shader.ShaderTemplate.Id}]");

            PopulateFromBitmapReferences(map, mat, shader);
            PopulateFromBitmapInfos(map, mat, shader);
        }

        private static void PopulateFromBitmapInfos(H2vMap map, Material<BitmapTag> mat, ShaderTag shader)
        {
            if (shader.BitmapInfos.Length > 0)
            {
                var bitms = shader.BitmapInfos[0];

                if (bitms == null)
                    return;

                if(map.TryGetTag(bitms.DiffuseBitmap, out var diffuse))
                {
                    mat.DiffuseMap = diffuse;
                }

                if (map.TryGetTag(bitms.AlphaBitmap, out var alpha))
                {
                    mat.AlphaMap = alpha;
                }
            }
        }

        private static void PopulateFromBitmapReferences(H2vMap map, Material<BitmapTag> mat, ShaderTag shader)
        {
            var args = shader.Arguments.First();
            var bitmRefs = args.ShaderMaps;


            for (int i = 0; i < bitmRefs.Length; i++)
            {
                var bitmRef = bitmRefs[i];

                var scale = Vector4.One;

                if(args.ShaderInputs.Length >= bitmRefs.Length)
                {
                    scale = args.ShaderInputs[i];
                }

                if (map.TryGetTag(bitmRef.Bitmap, out var bitm) == false)
                {
                    continue;
                }

                if(bitm == mat.DiffuseMap)
                {
                    continue;
                }

                if (bitm.TextureUsage == TextureUsage.Bump)
                {
                    mat.NormalMap = bitm;
                }

                if(bitm.TextureUsage == TextureUsage.Diffuse)
                {
                    if (mat.DiffuseMap == null)
                    {
                        mat.DiffuseMap = bitm;
                        continue;
                    }
                }

                if (bitm.TextureUsage == TextureUsage.Diffuse || bitm.TextureUsage == TextureUsage.Detail)
                {

                    // HACK: if texture is small, it's likely not a detail map
                    if (bitm.Width <= 8 || bitm.Height <= 8)
                        continue;


                    // HACK: blacklisted textures that likely are not detail maps:
                    if(
                        // linear_corner_fade
                        bitm.ID == 3784845107u ||
                        // default_detail
                        bitm.ID == 3783272219u
                    )
                    {
                        continue;
                    }

                    if (mat.DetailMap1 == null)
                    {
                        mat.DetailMap1 = bitm;
                        mat.Detail1Scale = scale;
                        continue;
                    }
                    else if (mat.DetailMap2 == null)
                    {
                        mat.DetailMap2 = bitm;
                        mat.Detail2Scale = scale;
                        continue;
                    }
                }
            }
        }

        private static void TwoAddEnvIllum(H2vMap map, ShaderTag.ShaderArguments shader, Material<BitmapTag> mat)
        {
            if(map.TryGetTag(shader.ShaderMaps[3].Bitmap, out var emissive))
            {
                mat.EmissiveMap = emissive;
            }

            if (map.TryGetTag(shader.ShaderMaps[2].Bitmap, out var diffuse))
            {
                mat.DiffuseMap = diffuse;
            }
        }

        private static void TexBumpPlasmaOneChannelIllum(H2vMap map, ShaderTag.ShaderArguments shader, Material<BitmapTag> mat)
        {
            if (map.TryGetTag(shader.ShaderMaps[1].Bitmap, out var one))
                mat.DiffuseMap = one;

            if (map.TryGetTag(shader.ShaderMaps[2].Bitmap, out var two))
            {
                mat.DetailMap1 = two;
                mat.Detail1Scale = shader.ShaderInputs[3];
            }

            if (map.TryGetTag(shader.ShaderMaps[3].Bitmap, out var emissive))
            {
                mat.EmissiveMap = emissive;
            }
        }

        private static void PrtSimple(H2vMap map, ShaderTag.ShaderArguments shader, Material<BitmapTag> mat)
        {
            if (map.TryGetTag(shader.ShaderMaps[0].Bitmap, out var zero))
                mat.NormalMap = zero;

            if (map.TryGetTag(shader.ShaderMaps[1].Bitmap, out var one))
                mat.DiffuseMap = one;

            if (map.TryGetTag(shader.ShaderMaps[2].Bitmap, out var two))
            {
                mat.DetailMap1 = two;
                mat.Detail1Scale = shader.ShaderInputs[3];
            }
        }

        private static void Overlay(H2vMap map, ShaderTag.ShaderArguments shader, Material<BitmapTag> mat)
        {
            // TODO: special overlay flag to enable discard for middle gray
            if (map.TryGetTag(shader.ShaderMaps[0].Bitmap, out var zero))
            {
                mat.DiffuseMap = zero;
                mat.AlphaMap = zero;
            }
        }

        private static void TexEnv(H2vMap map, ShaderTag.ShaderArguments shader, Material<BitmapTag> mat)
        {
            if (map.TryGetTag(shader.ShaderMaps[0].Bitmap, out var zero))
                mat.DiffuseMap = zero;

            if (map.TryGetTag(shader.ShaderMaps[1].Bitmap, out var one))
            {
                mat.DetailMap1 = one;
                mat.Detail1Scale = shader.ShaderInputs[3];
            }

            if (map.TryGetTag(shader.ShaderMaps[2].Bitmap, out var two))
                mat.SpecularMap = two;
        }

        private static void TexBumpDetailKeep(H2vMap map, ShaderTag.ShaderArguments shader, Material<BitmapTag> mat)
        {
            if (map.TryGetTag(shader.ShaderMaps[0].Bitmap, out var zero))
                mat.NormalMap = zero;

            if (map.TryGetTag(shader.ShaderMaps[1].Bitmap, out var one))
                mat.DiffuseMap = one;

            if (map.TryGetTag(shader.ShaderMaps[2].Bitmap, out var two))
            {
                mat.DetailMap1 = two;
                mat.Detail1Scale = shader.ShaderInputs[2];
            }
        }

        private static void TexBumpDetailBlend(H2vMap map, ShaderTag.ShaderArguments shader, Material<BitmapTag> mat)
        {
            if (map.TryGetTag(shader.ShaderMaps[0].Bitmap, out var zero))
                mat.NormalMap = zero;

            if (map.TryGetTag(shader.ShaderMaps[1].Bitmap, out var one))
                mat.DiffuseMap = one;

            if (map.TryGetTag(shader.ShaderMaps[2].Bitmap, out var two))
            {
                mat.DetailMap1 = two;
                mat.Detail1Scale = shader.ShaderInputs[2];
            }

            if (map.TryGetTag(shader.ShaderMaps[3].Bitmap, out var three))
            {
                mat.DetailMap2 = three;
                mat.Detail2Scale = shader.ShaderInputs[3];
            }
        }

        private static void TexBumpDetailKeepBlend(H2vMap map, ShaderTag.ShaderArguments shader, Material<BitmapTag> mat)
            => TexBumpDetailBlend(map, shader, mat);

        private static void TexBump(H2vMap map, ShaderTag.ShaderArguments shader, Material<BitmapTag> mat)
        {
            if (map.TryGetTag(shader.ShaderMaps[0].Bitmap, out var zero))
                mat.NormalMap = zero;

            if (map.TryGetTag(shader.ShaderMaps[2].Bitmap, out var two))
                mat.DiffuseMap = two;

            if (map.TryGetTag(shader.ShaderMaps[3].Bitmap, out var three))
            {
                mat.DetailMap1 = three;
                mat.Detail1Scale = shader.ShaderInputs[3];
            }
        }

        private static void TexBumpIllum(H2vMap map, ShaderTag.ShaderArguments shader, Material<BitmapTag> mat)
        {
            if (map.TryGetTag(shader.ShaderMaps[0].Bitmap, out var zero))
                mat.NormalMap = zero;

            if (map.TryGetTag(shader.ShaderMaps[1].Bitmap, out var one))
                mat.DiffuseMap = one;

            if (map.TryGetTag(shader.ShaderMaps[3].Bitmap, out var emissive))
            {
                mat.EmissiveMap = emissive;
            }
        }

        private static void TexBumpNoAlpha(H2vMap map, ShaderTag.ShaderArguments shader, Material<BitmapTag> mat) 
            => TexBump(map, shader, mat);

        private static void TexBumpEnvCombined(H2vMap map, ShaderTag.ShaderArguments shader, Material<BitmapTag> mat)
            => TexBump(map, shader, mat);

        private static void TransparentOneAlphaEnv(H2vMap map, ShaderTag.ShaderArguments shader, Material<BitmapTag> mat)
        {
            if (map.TryGetTag(shader.ShaderMaps[0].Bitmap, out var zero))
                mat.SpecularMap = zero;

            if (map.TryGetTag(shader.ShaderMaps[1].Bitmap, out var one))
                mat.DiffuseMap = one;

            if (map.TryGetTag(shader.ShaderMaps[2].Bitmap, out var two))
                mat.AlphaMap = two;
        }

        private static void TransparentTwoAlphaClouds(H2vMap map, ShaderTag.ShaderArguments shader, Material<BitmapTag> mat)
        {
            if (map.TryGetTag(shader.ShaderMaps[0].Bitmap, out var zero))
                mat.DiffuseMap = zero;

            if (map.TryGetTag(shader.ShaderMaps[1].Bitmap, out var one))
                mat.AlphaMap = one;
        }

        private static void TransparentPlasmaAlpha(H2vMap map, ShaderTag.ShaderArguments shader, Material<BitmapTag> mat)
        {
            if (map.TryGetTag(shader.ShaderMaps[0].Bitmap, out var zero))
                mat.DiffuseMap = zero;

            if (map.TryGetTag(shader.ShaderMaps[1].Bitmap, out var one))
                mat.AnimationMap = one;

            if (map.TryGetTag(shader.ShaderMaps[2].Bitmap, out var two))
                mat.AlphaMap = two;
        }

        private static void SkyTwoAlphaClouds(H2vMap map, ShaderTag.ShaderArguments shader, Material<BitmapTag> mat)
        {
            if (map.TryGetTag(shader.ShaderMaps[0].Bitmap, out var zero))
                mat.DiffuseMap = zero;
        }
    }
}
