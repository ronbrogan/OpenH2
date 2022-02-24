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

namespace OpenH2.Rendering.Vulkan
{
    public sealed unsafe class VulkanHost : IGraphicsHost, IGameLoopSource, IDisposable
    {
        private VulkanGraphicsAdapter adapter;
        internal IWindow window;
        private Action<double> updateAction;
        private Action<double> renderAction;

        public Vector2 ViewportSize => new Vector2(1600, 900);

        public bool AspectRatioChanged => false;

        public float AspectRatio => 16f / 9f;

        public VulkanHost()
        {
        }

        public void CreateWindow(Vector2 size, bool hidden = false)
        {
            var options = new WindowOptions(ViewOptions.DefaultVulkan);
            options.Size = new Vector2D<int>((int)size.X, (int)size.Y);

            this.window = Window.Create(options);
            this.window.Initialize();

            if(this.window.VkSurface is null)
            {
                throw new NotSupportedException("Unable to use Vulkan!");
            }

            this.adapter = new VulkanGraphicsAdapter(this, this.window.VkSurface);
        }

        public void EnableConsoleDebug()
        {
        }

        public IGraphicsAdapter GetGraphicsAdapter() => this.adapter;

        public void RegisterCallbacks(Action<double> updateCallback, Action<double> renderCallback)
        {
            //this.updateAction = updateCallback;
            this.renderAction = renderCallback;
        }

        public void Start(int updatesPerSecond, int framesPerSecond)
        {
            var sw = new Stopwatch();
            
            this.window.Run(() =>
            {
                this.window.DoEvents();
                var deltaT = sw.Elapsed.TotalMilliseconds;
                //this.updateAction(deltaT);
                //this.renderAction(deltaT);

                this.adapter.BeginFrame(new GlobalUniform());
                this.adapter.EndFrame();

                sw.Restart();
            });
        }

        public void Dispose()
        {
            this.adapter?.Dispose();
        }
    }
}
