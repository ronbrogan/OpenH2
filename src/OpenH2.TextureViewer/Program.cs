using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using OpenH2.Core.Factories;
using OpenH2.Core.Maps.Vista;
using OpenH2.Core.Tags;
using OpenH2.Foundation;
using OpenH2.Rendering.OpenGL;
using OpenH2.Rendering.Shaders;
using Silk.NET.Input;
using Silk.NET.Input.Extensions;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Shader = OpenH2.Rendering.Shaders.Shader;

namespace OpenH2.TextureViewer
{
    class Program
    {
        private static BitmapTag[] Bitmaps { get; set; }
        private static int CurrentBitmap { get; set; } = 0;

        private static Dictionary<int, int> BitmTextureIdLookup = new Dictionary<int, int>();

        public static uint MatriciesUniformHandle;
        public static GlobalUniform MatriciesUniform;
        private static uint QuadMeshId;
        private static Mesh<BitmapTag> quadMesh;
        private static uint ShaderHandle;
        private static OpenGLTextureBinder textureBinder;
        private static IWindow window;
        private static IInputContext input;
        private static GL gl;

        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("Must provide 1 argument: map path");
                return;
            }

            var mapPath = args[0];

            if (File.Exists(mapPath) == false)
            {
                Console.WriteLine($"Error: Could not find {mapPath}");
                return;
            }

            var factory = new MapFactory(Path.GetDirectoryName(mapPath));
            var h2map = factory.Load(Path.GetFileName(mapPath));

            if (h2map is not H2vMap scene)
            {
                throw new NotSupportedException("Only Vista maps are supported");
            }

            Bitmaps = scene.GetLocalTagsOfType<BitmapTag>().ToArray();

            var host = new OpenGLHost();
            host.CreateWindow(new Vector2(1600, 900));
            window = host.GetWindow();
            input = window.CreateInput();
            Setup(host);

            host.RegisterCallbacks(Update, Render);
            host.Start(30, 30);
        }

        private static DebugProc callback = DebugCallbackF;
        public static void Setup(OpenGLHost host)
        {
            textureBinder = new OpenGLTextureBinder(host);
            gl = GL.GetApi(host.GetWindow());

            gl.DebugMessageCallback(callback, (IntPtr.Zero));

            gl.Enable(EnableCap.DebugOutput);
            gl.Enable(EnableCap.DepthTest);
            gl.Enable(EnableCap.Multisample);
            gl.Enable(EnableCap.CullFace);
            gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            UploadQuadMesh();
            MatriciesUniform = new GlobalUniform()
            {
                ProjectionMatrix = Matrix4x4.CreateOrthographic(3.55555f, 2, 0, 10),
                ViewMatrix = Matrix4x4.Identity,
                ViewPosition = Vector3.Zero
            };

            ShaderHandle = OpenGLShaderCompiler.CreateShader(Shader.TextureViewer);
        }

        static KeyboardState keyboardState, lastKeyboardState;
        private static void Update(double time)
        {
            // read button down, increment CurrentBitmap
            var state = input.CaptureState();
            keyboardState = state.Keyboards[0];
            if (KeyPress(Key.Left))
            {
                SetNextBitmap(-1);
            }
            if (KeyPress(Key.Right))
            {
                SetNextBitmap(1);
            }

            lastKeyboardState = keyboardState;

            if (BitmTextureIdLookup.TryGetValue(CurrentBitmap, out var handle) == false)
            {
                var bitm = Bitmaps[CurrentBitmap];
                handle = textureBinder.GetOrBind(bitm, out var _);
                BitmTextureIdLookup[CurrentBitmap] = handle;
            }

            gl.ActiveTexture(TextureUnit.Texture0);
            gl.BindTexture(GLEnum.Texture2D, (uint)handle);
        }

        public static bool KeyPress(Key key)
        {
            return (keyboardState.IsKeyPressed(key) && (keyboardState.IsKeyPressed(key) != lastKeyboardState.IsKeyPressed(key)));
        }

        private static void SetNextBitmap(int offset)
        {
            BitmapTag candidate;

            do
            {
                CurrentBitmap += offset;

                if (CurrentBitmap == Bitmaps.Length)
                {
                    CurrentBitmap = 0;
                }

                if (CurrentBitmap == -1)
                {
                    CurrentBitmap = Bitmaps.Length - 1;
                }

                candidate = Bitmaps[CurrentBitmap];
            }
            while (candidate.TextureInfos[0].LevelsOfDetail[0].Data.Length == 0);

            Console.WriteLine("[" + CurrentBitmap + "] @ " + candidate.TextureInfos[0].ID + ", " + candidate.Name);
        }

        private static void Render(double time)
        {
            gl.UseProgram(ShaderHandle);

            SetupMatrixUniform();

            gl.BindVertexArray(QuadMeshId);

            var type = quadMesh.ElementType;
            var indicies = quadMesh.Indicies;

            switch (type)
            {
                case MeshElementType.TriangleList:
                    gl.DrawElements(GLEnum.Triangles, (uint)indicies.Length, GLEnum.UnsignedInt, 0);
                    break;
                case MeshElementType.TriangleStrip:
                    gl.DrawElements(GLEnum.TriangleStrip, (uint)indicies.Length, GLEnum.UnsignedInt, 0);
                    break;
            }
        }

        public static void UploadQuadMesh()
        {
            var mesh = new Mesh<BitmapTag>();
            mesh.Verticies = new VertexFormat[]
            {
                new VertexFormat(new Vector3(-1,-1,1), new Vector2(0,1), Vector3.Zero),
                new VertexFormat(new Vector3(-1,1,1), new Vector2(0,0), Vector3.Zero),
                new VertexFormat(new Vector3(1,1,1), new Vector2(1,0), Vector3.Zero),
                new VertexFormat(new Vector3(1,-1,1), new Vector2(1,1), Vector3.Zero),
            };

            mesh.Indicies = new int[] { 2,1,0, 3,2,0 };
            mesh.ElementType = MeshElementType.TriangleList;
            quadMesh = mesh;

            var verticies = mesh.Verticies;
            var indicies = mesh.Indicies;

            uint vao, vbo, ibo;

            gl.GenVertexArrays(1, out vao);
            gl.BindVertexArray(vao);

            gl.GenBuffers(1, out vbo);
            gl.BindBuffer(GLEnum.ArrayBuffer, vbo);
            gl.BufferData<VertexFormat>(GLEnum.ArrayBuffer, (nuint)(verticies.Length * VertexFormat.Size), verticies, GLEnum.StaticDraw);

            gl.GenBuffers(1, out ibo);
            gl.BindBuffer(GLEnum.ElementArrayBuffer, ibo);
            gl.BufferData<int>(GLEnum.ElementArrayBuffer, (nuint)(indicies.Length * sizeof(uint)), indicies, GLEnum.StaticDraw);

            SetupVertexFormatAttributes();

            QuadMeshId = vao;
        }

        private static unsafe void SetupVertexFormatAttributes()
        {
            // Attributes for VertexFormat.Position
            gl.EnableVertexAttribArray(0);
            gl.VertexAttribPointer(0, 3, GLEnum.Float, false, (uint)VertexFormat.Size, (void*)0);

            // Attributes for VertexFormat.TexCoords
            gl.EnableVertexAttribArray(1);
            gl.VertexAttribPointer(1, 2, GLEnum.Float, false, (uint)VertexFormat.Size, (void*)12);

            // Attributes for VertexFormat.Normal
            gl.EnableVertexAttribArray(2);
            gl.VertexAttribPointer(2, 3, GLEnum.Float, false, (uint)VertexFormat.Size, (void*)20);

            // Attributes for VertexFormat.Tangent
            gl.EnableVertexAttribArray(3);
            gl.VertexAttribPointer(3, 3, GLEnum.Float, false, (uint)VertexFormat.Size, (void*)32);

            // Attributes for VertexFormat.Bitangent
            gl.EnableVertexAttribArray(4);
            gl.VertexAttribPointer(4, 3, GLEnum.Float, false, (uint)VertexFormat.Size, (void*)44);
        }

        private static void SetupMatrixUniform()
        {
            if (MatriciesUniformHandle == default(int))
                gl.GenBuffers(1, out MatriciesUniformHandle);

            gl.BindBuffer(GLEnum.UniformBuffer, MatriciesUniformHandle);

            gl.BufferData(GLEnum.UniformBuffer, (uint)GlobalUniform.Size, MatriciesUniform, GLEnum.DynamicDraw);

            gl.BindBufferBase(GLEnum.UniformBuffer, 0, MatriciesUniformHandle);
            gl.BindBuffer(GLEnum.UniformBuffer, 0);
        }

        public static void DebugCallbackF(GLEnum source, GLEnum type, int id, GLEnum severity, int length, IntPtr message, IntPtr userParam)
        {
            if (severity == GLEnum.DebugSeverityNotification)
                return;

            string msg = Marshal.PtrToStringAnsi(message, length);
            Console.WriteLine(msg);
        }
    }
}
