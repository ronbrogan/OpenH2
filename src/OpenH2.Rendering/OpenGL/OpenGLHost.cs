using OpenH2.Foundation.Engine;
using OpenH2.Rendering.Abstractions;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace OpenH2.Rendering.OpenGL
{
    public class OpenGLHost : IGraphicsHost, IGameLoopSource, IDisposable
    {
        internal GL gl;
        private IGraphicsAdapter adapter;
        private IWindow window;
        private IInputContext inputContext;

        public bool AspectRatioChanged { get; private set; }

        public float AspectRatio { get; private set; }

        public System.Numerics.Vector2 ViewportSize { get; private set; }

        public OpenGLHost()
        {
        }

        public IGraphicsAdapter GetGraphicsAdapter()
        {
            return adapter;
        }

        // TODO: this is for input, abstract that system to remove this leak
        public IWindow GetWindow()
        {
            return window;
        }

        public IInputContext GetInputContext()
        {
            return this.inputContext;
        }

        private ManualResetEventSlim loaded = new ManualResetEventSlim();
        public void CreateWindow(System.Numerics.Vector2 size, bool hidden = false)
        {
            var options = new WindowOptions(ViewOptions.DefaultVulkan);
            options.Size = new Vector2D<int>((int)size.X, (int)size.Y);
            options.API = new GraphicsAPI
            {
                API = ContextAPI.OpenGL,
                Profile = ContextProfile.Core,
                Version = new APIVersion(4, 5)
            };

            this.window = Window.Create(options);
            this.window.Load += Window_Load;
            

            this.window.Resize += a =>
            {
                this.ViewportSize = new System.Numerics.Vector2(a.X, a.Y);
                this.AspectRatio = a.X / (float)a.Y;
                this.AspectRatioChanged = true;
            };

            window.Resize += this.Window_Resize;
            this.AspectRatio = size.X / size.Y;
            this.AspectRatioChanged = true;

            window.IsVisible = !hidden;

            //window.Closing += this.Window_Closed;
            this.window.Initialize();
            loaded.Wait();
        }

        private void Window_Load()
        {
            this.gl = GL.GetApi(this.window);
            this.inputContext = this.window.CreateInput();
            this.adapter = new OpenGLGraphicsAdapter(this);

            gl.Enable(EnableCap.DepthTest);
            gl.Enable(EnableCap.CullFace);
            gl.Enable(GLEnum.Alpha);
            gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            gl.Enable(EnableCap.Blend);
            loaded.Set();
        }

        private void Window_Closed()
        {
            Environment.Exit(0);
        }

        private void Window_Resize(Vector2D<int> a)
        {
            gl.Viewport(0, 0, (uint)a.X, (uint)a.Y);
            this.ViewportSize = new System.Numerics.Vector2(a.X, a.Y);
            this.AspectRatio = a.X / (float)a.Y;
            this.AspectRatioChanged = true;
        }

        public void RegisterCallbacks(Action<double> updateCallback, Action<double> renderCallback)
        {
            window.Update += f => updateCallback(f);
            window.Render += f =>
            {
                gl.ClearColor(0.2f, 0.2f, 0.2f, 1f);
                gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                renderCallback(f);
                window.SwapBuffers();
            };
        }

        public void Start(int updatesPerSecond, int framesPerSecond)
        {
            window.UpdatesPerSecond = updatesPerSecond;
            window.FramesPerSecond = framesPerSecond;
            window.Run();
        }

        public void EnableConsoleDebug()
        {
            gl.Enable(EnableCap.DebugOutput);

            gl.DebugMessageCallback(callbackWrapper, IntPtr.Zero);
        }

        private static DebugProc callbackWrapper = DebugCallbackF;

        private static void DebugCallbackF(GLEnum source, GLEnum type, int id, GLEnum severity, int length, IntPtr message, IntPtr userParam)
        {
            if ((DebugSeverity)severity == DebugSeverity.DebugSeverityNotification)
                return;

            string msg = Marshal.PtrToStringAnsi(message, length);
            Console.WriteLine(msg);
        }

        public void Dispose()
        {
            this.window?.Dispose();
        }
    }
}
