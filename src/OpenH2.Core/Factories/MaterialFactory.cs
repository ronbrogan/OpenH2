using OpenBlam.Core.FileSystem;
using OpenH2.Core.Configuration;
using OpenH2.Core.Enums.Texture;
using OpenH2.Core.Extensions;
using OpenH2.Core.Maps.Vista;
using OpenH2.Core.Tags;
using OpenH2.Core.Tags.Common.Models;
using OpenH2.Foundation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text.Json;

namespace OpenH2.Core.Factories
{
    public sealed class MaterialFactory : IDisposable, IMaterialFactory
    {
        private FileWatcher configWatcher;
        private MaterialMappingConfig mappingConfig;
        private List<Action> callbacks = new List<Action>();
        private HashSet<uint> stemsWarned = new HashSet<uint>();
        private Dictionary<uint, Material<BitmapTag>> createdMaterials = new Dictionary<uint, Material<BitmapTag>>();

        public MaterialFactory(string configRoot)
        {
            var configPath = Path.Combine(configRoot, ConfigurationConstants.MaterialConfigName);

            if (File.Exists(configPath) == false)
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
            if (createdMaterials.TryGetValue(mesh.Shader.Id, out var mat))
            {
                return mat;
            }

            mat = new Material<BitmapTag>
            {
                DiffuseColor = VectorExtensions.RandomColor()
            };

            if (map.TryGetTag(mesh.Shader, out var shader) == false)
            {
                return mat;
            }

            var args = shader.Arguments[0];
            var templateTag = map.GetTag(args.ShaderTemplate);
            var templateKey = templateTag.Name;

            if (mappingConfig.Aliases.TryGetValue(templateKey, out var alias))
            {
                templateKey = alias.Alias;
            }

            if (mappingConfig.Mappings.TryGetValue(templateKey, out var mapping))
            {
                PopulateFromMapping(map, mat, args, mapping);
                return mat;
            }

            if (stemsWarned.Contains(shader.ShaderTemplate.Id) == false)
            {
                Console.WriteLine($"Using heuristic for shader '{shader.Name}' stem[{shader.ShaderTemplate.Id}]");
                stemsWarned.Add(shader.ShaderTemplate.Id);
            }

            PopulateFromHeuristic(map, mat, shader);

            return mat;
        }

        private void PopulateFromMapping(
            H2vMap map,
            Material<BitmapTag> mat,
            ShaderTag.ShaderTemplateArguments args,
            MaterialMapping mapping)
        {
            mat.NormalMap = args.GetBitmap(map, mapping.NormalMapIndex);
            mat.NormalMapScale = args.GetInput(mapping.NormalScaleIndex);

            mat.DiffuseMap = args.GetBitmap(map, mapping.DiffuseMapIndex);

            if (mapping.DiffuseColor.Length == 4)
            {
                mat.DiffuseColor = new Vector4(mapping.DiffuseColor[0], mapping.DiffuseColor[1], mapping.DiffuseColor[2], mapping.DiffuseColor[3]);
            }
            else if (mapping.DiffuseColor.Length == 1)
            {
                mat.DiffuseColor = new Vector4(mapping.DiffuseColor[0]);
            }

            mat.DetailMap1 = args.GetBitmap(map, mapping.Detail1MapIndex);
            mat.Detail1Scale = args.GetInput(mapping.Detail1ScaleIndex);

            mat.DetailMap2 = args.GetBitmap(map, mapping.Detail2MapIndex);
            mat.Detail2Scale = args.GetInput(mapping.Detail2ScaleIndex);

            mat.EmissiveMap = args.GetBitmap(map, mapping.EmissiveMapIndex);
            mat.EmissiveArguments = args.GetInput(mapping.EmissiveArgumentsIndex);
            mat.EmissiveType = mapping.EmissiveType;

            mat.AlphaMap = args.GetBitmap(map, mapping.AlphaMapIndex);
            mat.AlphaFromRed = mapping.AlphaFromRed;

            mat.ColorChangeMask = args.GetBitmap(map, mapping.ColorChangeMaskMapIndex);

            mat.SpecularMap = args.GetBitmap(map, mapping.SpecularMapIndex);
        }

        private static void PopulateFromHeuristic(H2vMap map, Material<BitmapTag> mat, ShaderTag shader)
        {
            foreach (var info in shader.BitmapInfos)
            {
                if (info.AlphaBitmap.IsInvalid == false && mat.AlphaMap == default)
                {
                    map.TryGetTag(info.AlphaBitmap, out var alpha);
                    mat.AlphaMap = alpha;
                }

                if (info.DiffuseBitmap.IsInvalid == false && mat.DiffuseMap == default)
                {
                    map.TryGetTag(info.DiffuseBitmap, out var diff);
                    mat.DiffuseMap = diff;
                }
            }

            var args = shader.Arguments[0];
            var bitmRefs = args.BitmapArguments;

            for (int i = 0; i < bitmRefs.Length; i++)
            {
                var bitmRef = bitmRefs[i];

                if (map.TryGetTag(bitmRef.Bitmap, out var bitm) == false)
                {
                    continue;
                }

                if (bitm == mat.DiffuseMap || bitm == mat.AlphaMap)
                {
                    continue;
                }

                if (bitm.TextureUsage == TextureUsage.Bump || bitm.Name.Contains("bump"))
                {
                    mat.NormalMap = bitm;
                    mat.NormalMapScale = args.GetInput(0, new Vector4(1, 1, 0, 0));
                    continue;
                }

                if (bitm.TextureUsage == TextureUsage.Diffuse)
                {
                    if (mat.DiffuseMap == default)
                    {
                        mat.DiffuseMap = bitm;
                        continue;
                    }
                }

                if (bitm.TextureUsage == TextureUsage.Diffuse || bitm.TextureUsage == TextureUsage.Detail || bitm.Name.Contains("detail"))
                {

                    // HACK: if texture is small, it's likely not a detail map
                    if (bitm.TextureInfos[0].Width <= 16 || bitm.TextureInfos[0].Height <= 16)
                        continue;

                    int inputOffset = i;
                    Vector4 detailScale = Vector4.Zero;

                    while ((detailScale.X < 1 || detailScale.Y < 1 || detailScale.Z != 0 || detailScale.W != 0)
                        && inputOffset < args.ShaderInputs.Length)
                    {
                        detailScale = args.ShaderInputs[inputOffset];
                        inputOffset++;
                    }

                    if (mat.DetailMap1 == null)
                    {
                        mat.DetailMap1 = bitm;
                        mat.Detail1Scale = detailScale;
                        continue;
                    }
                    else if (mat.DetailMap2 == null)
                    {
                        mat.DetailMap2 = bitm;
                        mat.Detail2Scale = detailScale;
                        continue;
                    }
                }
            }
        }

        private void ReadMaterialMappingConfig(string configPath)
        {
            var json = File.ReadAllText(configPath);
            var opts = new JsonSerializerOptions()
            {
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                PropertyNameCaseInsensitive = true
            };

            this.mappingConfig = JsonSerializer.Deserialize<MaterialMappingConfig>(json, opts);

            foreach (var cb in callbacks)
                cb();
        }

        public void Dispose()
        {
            this.configWatcher?.Dispose();
        }
    }
}
