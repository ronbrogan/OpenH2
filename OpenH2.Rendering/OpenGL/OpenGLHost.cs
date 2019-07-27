using System;
using OpenH2.Rendering.Abstractions;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace OpenH2.Rendering.OpenGL
{
    public class OpenGLHost : IGraphicsHost, IGameLoopSource
    {
        private readonly IGraphicsAdapter adapter;
        private GameWindow window;

        public OpenGLHost()
        {
            this.adapter = new OpenGLGraphicsAdapter();
        }

        public IGraphicsAdapter GetAdapter()
        {
            return adapter;
        }

        public void CreateWindow()
        {
            window = new GameWindow(800, 450, GraphicsMode.Default, "OpenH2", GameWindowFlags.Default, DisplayDevice.Default, 4, 0, GraphicsContextFlags.Debug);

            GL.Enable(EnableCap.DepthTest);
        }

        public void RegisterCallbacks(Action<double> updateCallback, Action<double> renderCallback)
        {
            window.UpdateFrame += (s, e) => updateCallback(e.Time);
            window.RenderFrame += (s, e) =>
            {
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                renderCallback(e.Time);
                window.SwapBuffers();
            };
        }

        public void Start(int updatesPerSecond, int framesPerSecond)
        {
            window.Run(updatesPerSecond, framesPerSecond);
        }
    }
}
