using OpenH2.Foundation.Engine;
using OpenH2.Rendering.Abstractions;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace OpenH2.Rendering.OpenGL
{
    public class OpenGLHost : IGraphicsHost, IGameLoopSource
    {
        private readonly IGraphicsAdapter adapter;
        private GameWindow window;

        public bool AspectRatioChanged { get; private set; }

        public float AspectRatio { get; private set; }

        public System.Numerics.Vector2 ViewportSize { get; private set; }

        public OpenGLHost()
        {
            this.adapter = new OpenGLGraphicsAdapter(this);
        }

        public IGraphicsAdapter GetGraphicsAdapter()
        {
            return adapter;
        }

        // TODO: this is for input, abstract that system to remove this leak
        public GameWindow GetWindow()
        {
            return window;
        }

        public void CreateWindow(System.Numerics.Vector2 size, bool hidden = false)
        {
            var settings = new GameWindowSettings()
            {
                IsMultiThreaded = false
            };

            var nsettings = new NativeWindowSettings()
            {
                API = ContextAPI.OpenGL,
                AutoLoadBindings = true,
                Size = new Vector2i((int)size.X, (int)size.Y),
                Title = "OpenH2",
                Flags = ContextFlags.Debug,
                APIVersion = new Version(4, 6),
                NumberOfSamples = 8
            };

            window = new GameWindow(settings, nsettings);

            window.Resize += this.Window_Resize;
            this.AspectRatio = size.X / size.Y;
            this.AspectRatioChanged = true;

            window.IsVisible = !hidden;

            window.Closed += this.Window_Closed;

            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.AlphaTest);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Enable(EnableCap.Blend);
        }

        private void Window_Closed()
        {
            Environment.Exit(0);
        }

        private void Window_Resize(ResizeEventArgs a)
        {
            GL.Viewport(0, 0, a.Width, a.Height);
            this.ViewportSize = new System.Numerics.Vector2(a.Width, a.Height);
            this.AspectRatio = a.Width / (float)a.Height;
            this.AspectRatioChanged = true;
        }

        public void RegisterCallbacks(Action<double> updateCallback, Action<double> renderCallback)
        {
            window.UpdateFrame += f => updateCallback(f.Time);
            window.RenderFrame += f =>
            {
                GL.ClearColor(0.2f, 0.2f, 0.2f, 1f);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);


                renderCallback(f.Time);
                window.SwapBuffers();
            };
        }

        public void Start(int updatesPerSecond, int framesPerSecond)
        {
            window.UpdateFrequency = updatesPerSecond;
            window.RenderFrequency = framesPerSecond;
            window.Run();
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

        public IEnumerable<string> ListSupportedExtensions()
        {
            var count = GL.GetInteger(GetPName.NumExtensions);

            for(var i = 0; i < count; i++)
                yield return GL.GetString(StringNameIndexed.Extensions, i);
        }
    }
}
