using OpenH2.Core.Extensions;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;
using System;
using System.Text;

namespace OpenH2.Rendering.Vulkan.Internals
{
    internal unsafe class VkInstance : VkObject, IDisposable
    {
        private string[] validationLayers = new[] { "VK_LAYER_KHRONOS_validation" };

        private readonly VulkanHost host;

        private Instance _instance;
        public Instance Instance => _instance;

        public VkInstance(VulkanHost host) : base(host.vk)
        {
            this.host = host;

            var extensionStrings = host.window.VkSurface.GetRequiredExtensions(out var reqExtensionCount);

            uint extensionCount = 0;
            vk.EnumerateInstanceExtensionProperties(0, ref extensionCount, null);
            var extensions = new ExtensionProperties[extensionCount];
            vk.EnumerateInstanceExtensionProperties(0, ref extensionCount, ref extensions[0]);

            Console.WriteLine("Extensions");
            foreach (var ext in extensions)
            {
                Console.WriteLine("\t" + Encoding.UTF8.GetNullTerminatedString(ext.ExtensionName, 128));
            }

            uint availableLayerCount = 0;
            vk.EnumerateInstanceLayerProperties(ref availableLayerCount, null);
            var layers = new LayerProperties[availableLayerCount];
            vk.EnumerateInstanceLayerProperties(ref availableLayerCount, ref layers[0]);

            var layerNames = stackalloc byte*[validationLayers.Length];
            for (var i = 0; i < validationLayers.Length; i++)
            {
                layerNames[i] = PinnedUtf8.Get(validationLayers[i]);
            }

            Console.WriteLine("Layers");
            foreach (var layer in layers)
            {
                Console.WriteLine("\t" + Encoding.UTF8.GetNullTerminatedString(layer.LayerName, 128));
            }

            var appInfo = new ApplicationInfo
            {
                SType = StructureType.ApplicationInfo,
                PApplicationName = PinnedUtf8.Get("OpenH2"),
                ApplicationVersion = Vk.MakeVersion(0, 0, 1),
                PEngineName = PinnedUtf8.Get("OpenH2.Engine"),
                EngineVersion = Vk.MakeVersion(0, 0, 1),
                ApiVersion = Vk.Version12
            };

            var instanceInfo = new InstanceCreateInfo
            {
                SType = StructureType.InstanceCreateInfo,
                PApplicationInfo = &appInfo,
                EnabledExtensionCount = reqExtensionCount,
                PpEnabledExtensionNames = extensionStrings,
                EnabledLayerCount = (uint)validationLayers.Length,
                PpEnabledLayerNames = layerNames
            };

            SUCCESS(vk.CreateInstance(in instanceInfo, null, out _instance), "Couldn't create instance");
        }

        public VkDevice CreateDevice()
        {
            return new VkDevice(host, this, validationLayers);
        }

        public static implicit operator Instance(VkInstance @this) => @this._instance;

        public void Dispose()
        {
            vk.DestroyInstance(_instance, null);
        }
    }
}
