using Newtonsoft.Json;
using OpenH2.Core.Configuration;
using OpenH2.Core.Enums.Texture;
using OpenH2.Core.Extensions;
using OpenH2.Core.Representations;
using OpenH2.Core.Tags;
using OpenH2.Core.Tags.Common;
using OpenH2.Foundation;
using OpenH2.Foundation.FileSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace OpenH2.Core.Factories
{
    public sealed class MaterialFactory : IDisposable
    {
        private FileWatcher configWatcher;
        private MaterialMappingConfig mappingConfig;
        private List<Action> callbacks = new List<Action>();

        public MaterialFactory(string configRoot)
        {
            var configPath = Path.Combine(configRoot, ConfigurationConstants.MaterialConfigName);

            if(File.Exists(configPath) == false)
            {
                throw new ArgumentException("Material config file does not exist.", nameof(configRoot));
            }

            ReadMaterialMappingConfig(configPath);

            configWatcher = new FileWatcher(configPath);
            configWatcher.AddListener(ReadMaterialMappingConfig);
        }

        public void AddListener(Action callback)
        {
            callbacks.Add(callback);
        }

        public Material<BitmapTag> CreateMaterial(H2vMap map, ModelMesh mesh)
        {
            var mat = new Material<BitmapTag>
            {
                DiffuseColor = VectorExtensions.RandomColor()
            };

            if (map.TryGetTag(mesh.Shader, out var shader) == false)
            {
                return mat;
            }

            var args = shader.Arguments[0];

            if (mappingConfig.Mappings.TryGetValue(args.ShaderTemplate.Id, out var mapping))
            {
                PopulateFromMapping(map, mat, args, mapping);
                return mat;
            }

            Console.WriteLine($"Using heuristic for shader '{shader.Name}' stem[{shader.ShaderTemplate.Id}]");

            PopulateFromBitmapReferences(map, mat, shader);
            PopulateFromBitmapInfos(map, mat, shader);

            return mat;
        }

        private void PopulateFromMapping(
            H2vMap map,
            Material<BitmapTag> mat,
            ShaderTag.ShaderArguments args,
            MaterialMapping mapping)
        {
            
            mat.NormalMap = args.GetBitmap(map, mapping.NormalMapIndex);
            mat.NormalMapScale = args.GetInput(mapping.NormalScaleIndex);

            mat.DiffuseMap = args.GetBitmap(map, mapping.DiffuseMapIndex);

            mat.DetailMap1 = args.GetBitmap(map, mapping.Detail1MapIndex);
            mat.Detail1Scale = args.GetInput(mapping.Detail1ScaleIndex);

            mat.DetailMap2 = args.GetBitmap(map, mapping.Detail2MapIndex);
            mat.Detail2Scale = args.GetInput(mapping.Detail2ScaleIndex);

            mat.EmissiveMap = args.GetBitmap(map, mapping.EmissiveMapIndex);
            mat.EmissiveArguments = args.GetInput(mapping.EmissiveArgumentsIndex);
            mat.EmissiveType = mapping.EmissiveType;

            mat.AlphaMap = args.GetBitmap(map, mapping.AlphaMapIndex);

            mat.SpecularMap = args.GetBitmap(map, mapping.SpecularMapIndex);

            if(mat.EmissiveType == EmissiveType.ThreeChannel)
            {
                mat.DiffuseColor = Vector4.Zero;
            }
        }

        private static void PopulateFromBitmapInfos(H2vMap map, Material<BitmapTag> mat, ShaderTag shader)
        {
            if (shader.BitmapInfos.Length > 0)
            {
                var bitms = shader.BitmapInfos[0];

                if (bitms == null)
                    return;

                if (map.TryGetTag(bitms.DiffuseBitmap, out var diffuse))
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
            var args = shader.Arguments[0];
            var bitmRefs = args.ShaderMaps;


            for (int i = 0; i < bitmRefs.Length; i++)
            {
                var bitmRef = bitmRefs[i];

                var scale = Vector4.One;

                if (args.ShaderInputs.Length >= bitmRefs.Length)
                {
                    scale = args.ShaderInputs[i];
                }

                if (map.TryGetTag(bitmRef.Bitmap, out var bitm) == false)
                {
                    continue;
                }

                if (bitm == mat.DiffuseMap)
                {
                    continue;
                }

                if (bitm.TextureUsage == TextureUsage.Bump)
                {
                    mat.NormalMap = bitm;
                }

                if (bitm.TextureUsage == TextureUsage.Diffuse)
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
                    if (
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

        private void ReadMaterialMappingConfig(string configPath)
        {
            var serializer = new JsonSerializer();
            using var textReader = new StreamReader(configPath);
            using var reader = new JsonTextReader(textReader);

            serializer.DefaultValueHandling = DefaultValueHandling.Ignore;

            this.mappingConfig = serializer.Deserialize<MaterialMappingConfig>(reader);

            foreach (var cb in callbacks)
                cb();

        }

        public void Dispose()
        {
            this.configWatcher?.Dispose();
        }
    }
}
