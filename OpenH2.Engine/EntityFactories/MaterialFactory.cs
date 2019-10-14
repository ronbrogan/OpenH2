using OpenH2.Core.Enums.Texture;
using OpenH2.Core.Representations;
using OpenH2.Core.Tags;
using OpenH2.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenH2.Engine.EntityFactories
{
    public class MaterialFactory
    {
        public static void PopulateMaterial(H2vMap map, Material<BitmapTag> mat, ShaderTag shader)
        {
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
            var bitmRefs = shader.Arguments.First().ShaderMaps;


            foreach (var bitmRef in bitmRefs)
            {
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
                    if (mat.DetailMap1 == null)
                    {
                        mat.DetailMap1 = bitm;
                        mat.Detail1Scale = bitmRef.ValueB;
                        continue;
                    }
                    else if (mat.DetailMap2 == null)
                    {
                        mat.DetailMap2 = bitm;
                        mat.Detail2Scale = bitmRef.ValueB;
                        continue;
                    }
                }
            }
        }
    }
}
