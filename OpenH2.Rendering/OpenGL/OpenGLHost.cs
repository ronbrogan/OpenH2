using System;
using System.Runtime.InteropServices;
using OpenH2.Foundation.Engine;
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

        public void CreateWindow(System.Numerics.Vector2 size, bool hidden = false)
        {
            window = new GameWindow((int)size.X, (int)size.Y, GraphicsMode.Default, "OpenH2", GameWindowFlags.Default, DisplayDevice.Default, 4, 0, GraphicsContextFlags.Debug);

            window.Visible = !hidden;

            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.AlphaTest);
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        }

        public void RegisterCallbacks(Action<double> updateCallback, Action<double> renderCallback)
        {
            window.UpdateFrame += (s, e) => updateCallback(e.Time);
            window.RenderFrame += (s, e) =>
            {
                GL.ClearColor(0.2f, 0.2f, 0.2f, 1f);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);


                renderCallback(e.Time);
                window.SwapBuffers();
            };
        }

        public void Start(int updatesPerSecond, int framesPerSecond)
        {
            window.Run(updatesPerSecond, framesPerSecond);
        }

        public void EnableConsoleDebug()
        {
            GL.Enable(EnableCap.DebugOutput);

            GL.DebugMessageCallback(callbackWrapper, IntPtr.Zero);
        }

        private static DebugProc callbackWrapper = DebugCallbackF;
        private static void DebugCallbackF(DebugSource source, DebugType type, int id, DebugSeverity severity, int length, IntPtr message, IntPtr userParam)
        {
            if (severity == DebugSeverity.DebugSeverityNotification)
                return;

            string msg = Marshal.PtrToStringAnsi(message, length);
            Console.WriteLine(msg);
        }
    }
}
