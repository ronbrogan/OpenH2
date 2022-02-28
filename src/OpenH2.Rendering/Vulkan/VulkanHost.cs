using OpenH2.Foundation.Engine;
using OpenH2.Rendering.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Silk.NET.Windowing;
using Silk.NET.Maths;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;
using System.Diagnostics;
using OpenH2.Rendering.Shaders;
using Silk.NET.Input;

namespace OpenH2.Rendering.Vulkan
{
    public sealed unsafe class VulkanHost : IGraphicsHost, IGameLoopSource, IDisposable
    {
        private VulkanGraphicsAdapter adapter;
        private IInputContext inputContext;
        internal IWindow window;
        

        public readonly Vk vk;

        public bool AspectRatioChanged { get; private set; }

        public float AspectRatio { get; private set; }

        public System.Numerics.Vector2 ViewportSize { get; private set; }

        public VulkanHost()
        {
            this.vk = Vk.GetApi();
        }

        public void CreateWindow(Vector2 size, bool hidden = false)
        {
            var options = new WindowOptions(ViewOptions.DefaultVulkan);
            options.Size = new Vector2D<int>((int)size.X, (int)size.Y);

            this.window = Window.Create(options);
            this.window.Initialize();
            this.inputContext = this.window.CreateInput();

            this.window.Resize += a =>
            {
                this.ViewportSize = new System.Numerics.Vector2(a.X, a.Y);
                this.AspectRatio = a.X / (float)a.Y;
                this.AspectRatioChanged = true;
            };

            if (this.window.VkSurface is null)
            {
                throw new NotSupportedException("Unable to use Vulkan!");
            }

            this.adapter = new VulkanGraphicsAdapter(this);
        }

        public IInputContext GetInputContext()
        {
            return this.inputContext;
        }

        public void EnableConsoleDebug()
        {
        }

        public IGraphicsAdapter GetGraphicsAdapter() => this.adapter;

        public void RegisterCallbacks(Action<double> updateCallback, Action<double> renderCallback)
        {
            this.window.Update += updateCallback;
            this.window.Render += renderCallback;
        }

        public void Start(int updatesPerSecond, int framesPerSecond)
        {
            this.window.UpdatesPerSecond = updatesPerSecond;
            this.window.FramesPerSecond = framesPerSecond;

            this.window.Run();
        }

        public void Dispose()
        {
            this.adapter?.Dispose();
        }
    }
}
