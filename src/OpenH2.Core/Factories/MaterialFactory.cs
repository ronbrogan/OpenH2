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
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

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

            uint templateId = args.ShaderTemplate.Id;

            if (mappingConfig.Aliases.TryGetValue(templateId, out var alias))
            {
                templateId = alias.Alias;
            }

            if (mappingConfig.Mappings.TryGetValue(templateId, out var mapping))
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

            opts.Converters.Add(new DictionaryUintTValueConverter());

            this.mappingConfig = JsonSerializer.Deserialize<MaterialMappingConfig>(json, opts);

            foreach (var cb in callbacks)
                cb();
        }

        public void Dispose()
        {
            this.configWatcher?.Dispose();
        }

        public class DictionaryUintTValueConverter : JsonConverterFactory
        {
            public override bool CanConvert(Type typeToConvert)
            {
                if (!typeToConvert.IsGenericType)
                {
                    return false;
                }

                if (typeToConvert.GetGenericTypeDefinition() != typeof(Dictionary<,>))
                {
                    return false;
                }

                return typeToConvert.GetGenericArguments()[0] == typeof(uint);
            }

            public override JsonConverter CreateConverter(
                Type type,
                JsonSerializerOptions options)
            {
                Type valueType = type.GetGenericArguments()[1];

                JsonConverter converter = (JsonConverter)Activator.CreateInstance(
                    typeof(DictionaryUintConverterInner<>).MakeGenericType(
                        new Type[] { valueType }),
                    BindingFlags.Instance | BindingFlags.Public,
                    binder: null,
                    args: new object[] { options },
                    culture: null);

                return converter;
            }

            private class DictionaryUintConverterInner<TValue> :
                JsonConverter<Dictionary<uint, TValue>>
            {
                private readonly JsonConverter<TValue> _valueConverter;
                private Type _valueType;

                public DictionaryUintConverterInner(JsonSerializerOptions options)
                {
                    // For performance, use the existing converter if available.
                    _valueConverter = (JsonConverter<TValue>)options
                        .GetConverter(typeof(TValue));

                    _valueType = typeof(TValue);
                }

                public override Dictionary<uint, TValue> Read(
                    ref Utf8JsonReader reader,
                    Type typeToConvert,
                    JsonSerializerOptions options)
                {
                    if (reader.TokenType != JsonTokenType.StartObject)
                    {
                        throw new JsonException();
                    }

                    Dictionary<uint, TValue> value = new Dictionary<uint, TValue>();

                    while (reader.Read())
                    {
                        if (reader.TokenType == JsonTokenType.EndObject)
                        {
                            return value;
                        }

                        // Get the key.
                        if (reader.TokenType != JsonTokenType.PropertyName)
                        {
                            throw new JsonException();
                        }

                        string propertyName = reader.GetString();

                        if (!uint.TryParse(propertyName, out var key))
                        {
                            throw new JsonException(
                                $"Unable to convert \"{propertyName}\" to UINT.");
                        }

                        // Get the value.
                        TValue v;
                        if (_valueConverter != null)
                        {
                            reader.Read();
                            v = _valueConverter.Read(ref reader, _valueType, options);
                        }
                        else
                        {
                            v = JsonSerializer.Deserialize<TValue>(ref reader, options);
                        }

                        // Add to dictionary.
                        value.Add(key, v);
                    }

                    throw new JsonException();
                }

                public override void Write(
                    Utf8JsonWriter writer,
                    Dictionary<uint, TValue> value,
                    JsonSerializerOptions options)
                {
                    writer.WriteStartObject();

                    foreach (KeyValuePair<uint, TValue> kvp in value)
                    {
                        writer.WritePropertyName(kvp.Key.ToString());

                        if (_valueConverter != null)
                        {
                            _valueConverter.Write(writer, kvp.Value, options);
                        }
                        else
                        {
                            JsonSerializer.Serialize(writer, kvp.Value, options);
                        }
                    }

                    writer.WriteEndObject();
                }
            }
        }
    }
}
