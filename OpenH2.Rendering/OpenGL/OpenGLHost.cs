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
            window = new GameWindow(600, 400, GraphicsMode.Default, "OpenH2", GameWindowFlags.Default, DisplayDevice.Default, 4, 0, GraphicsContextFlags.Debug);
        }

        public void RegisterCallbacks(Action updateCallback, Action renderCallback)
        {
            window.UpdateFrame += (s, e) => updateCallback();
            window.RenderFrame += (s, e) =>
            {
                renderCallback();
                window.SwapBuffers();
            };
        }

        public void Start(int updatesPerSecond, int framesPerSecond)
        {
            window.Run(updatesPerSecond, framesPerSecond);
        }
    }
}
