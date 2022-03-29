using System;
using System.Numerics;
using OpenH2.Foundation.Engine;
using OpenH2.Rendering.Abstractions;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Vulkan;
using Silk.NET.Windowing;

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

        public Vector2 ViewportSize { get; private set; }

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

        private ulong tick = 0;
        public void RegisterCallbacks(Action<double> updateCallback, Action<double> renderCallback)
        {
            this.window.Update += (d) =>
            {
                // First delta time includes time between window open and loop start, skipping that one for sanity
                //      - causes physics engine to step forward multiple seconds before everything is in the scene
                if (tick++ != 0) updateCallback(d);
            };
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
