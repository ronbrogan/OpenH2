using OpenH2.Rendering.Shaders;
using OpenH2.Rendering.Vulkan.Internals;
using Silk.NET.Vulkan;
using System;
using System.Collections.Concurrent;
using System.IO;


namespace OpenH2.Rendering.Vulkan
{

    internal class VulkanShaderCompiler : IDisposable
    {
        private ConcurrentDictionary<(Shader, ShaderType), VkShader> shaderCache = new();
        private VkDevice device;

        public VulkanShaderCompiler(VkDevice device)
        {
            this.device = device;
        }

        public VkShader? GetShader(Shader shader, ShaderType type, string entryPoint = "main")
        {
            if(shaderCache.TryGetValue((shader, type), out var instance))
            {
                return instance;
            }

            if (VulkanShaderCompiler.IsPresent(shader, type))
                instance = new VkShader(device, shader, type, entryPoint);

            shaderCache.TryAdd((shader, type), instance);
            return instance;
        }

        private static string GetPath(Shader shader, ShaderType type)
        {
            var shaderName = shader.ToString();
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Shaders", shaderName);

            var stub = type switch
            {
                ShaderType.Vertex => "vert",
                ShaderType.Fragment => "frag",
                ShaderType.Geometry => "geom",
                _ => throw new NotSupportedException($"Shader type {type} is not yet supported")
            };

            return Path.Combine(path, $"{shaderName}.vk.{stub}.spv");
        }

        public static bool IsPresent(Shader shader, ShaderType type)
        {
            return File.Exists(GetPath(shader, type));
        }

        public unsafe static ShaderModule LoadSpirvShader(VkDevice device, Shader shader, ShaderType type)
        {
            var bytes = File.ReadAllBytes(GetPath(shader, type));

            fixed (byte* ptr = bytes)
            {
                var createInfo = new ShaderModuleCreateInfo
                {
                    SType = StructureType.ShaderModuleCreateInfo,
                    CodeSize = (uint)bytes.Length,
                    PCode = (uint*)ptr
                };

                if (device.vk.CreateShaderModule(device, in createInfo, null, out var shaderModule) != Result.Success)
                    throw new Exception("Unable to compile shader");

                return shaderModule;
            }
        }

        public void Dispose()
        {
            foreach(var (_,shader) in this.shaderCache)
            {
                shader?.Dispose();
            }
        }
    }
}
