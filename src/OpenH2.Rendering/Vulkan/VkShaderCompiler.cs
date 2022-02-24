using Silk.NET.Vulkan;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenH2.Rendering.Vulkan
{
    internal class VkShaderCompiler
    {
        public static byte[] LoadSpirvBytes(string shaderName, string type = "vert")
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "Shaders", shaderName);

            if (Directory.Exists(path) == false)
            {
                throw new Exception("Couldn't find shader folder: " + path);
            }

            return File.ReadAllBytes(Path.Combine(path, type + ".spv"));
        }

        public unsafe static ShaderModule LoadSpirvShader(Vk vk, Device device, string shaderName, string type = "vert")
        {
            var bytes = LoadSpirvBytes(shaderName, type);

            fixed (byte* ptr = bytes)
            {
                var createInfo = new ShaderModuleCreateInfo
                {
                    SType = StructureType.ShaderModuleCreateInfo,
                    CodeSize = (uint)bytes.Length,
                    PCode = (uint*)ptr
                };

                if (vk.CreateShaderModule(device, in createInfo, null, out var shader) != Result.Success)
                    throw new Exception("Unable to compile shader");

                return shader;
            }
        }
    }
}
