using OpenH2.Core.Factories;
using OpenH2.Core.Representations;
using OpenH2.Rendering.OpenGL;
using OpenH2.Translation;
using OpenH2.Translation.TagData;
using System;
using System.IO;
using System.Linq;
using OpenTK.Graphics.OpenGL;
using OpenH2.Foundation;
using System.Numerics;
using OpenH2.Rendering.Shaders;
using OpenH2.Core.Tags;
using System.Runtime.InteropServices;
using OpenTK.Input;
using System.Collections.Generic;

namespace OpenH2.TextureViewer
{
    class Program
    {
        private static BitmapTag[] Bitmaps { get; set; }
        private static int CurrentBitmap { get; set; } = 0;

        private static Dictionary<int, int> BitmTextureIdLookup = new Dictionary<int, int>();

        public static int MatriciesUniformHandle;
        public static GlobalUniform MatriciesUniform;
        private static uint QuadMeshId;
        private static Mesh quadMesh;
        private static int ShaderHandle;
        private static OpenGLTextureBinder textureBinder = new OpenGLTextureBinder();

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

            H2vMap scene = null;

            using (var map = new FileStream(mapPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var factory = new MapFactory(Path.GetDirectoryName(mapPath));
                scene = factory.FromFile(map);
            }

            Bitmaps = scene.Tags.Where(t => t.Value is BitmapTag).Select(t => t.Value as BitmapTag).ToArray();

            var host = new OpenGLHost();
            host.CreateWindow();
            Setup();

            host.RegisterCallbacks(Update, Render);
            host.Start(30, 30);
        }

        private static DebugProc callback = DebugCallbackF;
        public static void Setup()
        {
            GL.DebugMessageCallback(callback, (IntPtr.Zero));

            GL.Enable(EnableCap.DebugOutput);
            GL.Enable(EnableCap.Blend);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Multisample);
            GL.Enable(EnableCap.CullFace);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            UploadQuadMesh();
            MatriciesUniform = new GlobalUniform()
            {
                ProjectionMatrix = Matrix4x4.CreateOrthographic(3.55555f, 2, 0, 10),
                ViewMatrix = Matrix4x4.Identity,
                ViewPosition = Vector3.Zero
            };

            ShaderHandle = ShaderCompiler.CreateShader("TextureViewer");
        }

        static KeyboardState keyboardState, lastKeyboardState;
        private static void Update(double time)
        {
            // read button down, increment CurrentBitmap
            keyboardState = Keyboard.GetState(0);
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
                handle = textureBinder.Bind(Bitmaps[CurrentBitmap], out var _);
                BitmTextureIdLookup[CurrentBitmap] = handle;
            }

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, handle);
        }

        public static bool KeyPress(Key key)
        {
            return (keyboardState[key] && (keyboardState[key] != lastKeyboardState[key]));
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

                candidate = Bitmaps[CurrentBitmap];
            }
            while (candidate.LevelsOfDetail[0].Data.Length == 0);

            Console.WriteLine("[" + CurrentBitmap + "] @ " + candidate.ID + ", " + candidate.Name);
        }

        private static void Render(double time)
        {
             GL.UseProgram(ShaderHandle);

            SetupMatrixUniform();

            GL.BindVertexArray(QuadMeshId);

            var type = quadMesh.ElementType;
            var indicies = quadMesh.Indicies;

            switch (type)
            {
                case MeshElementType.TriangleList:
                    GL.DrawElements(PrimitiveType.Triangles, indicies.Length, DrawElementsType.UnsignedInt, 0);
                    break;
                case MeshElementType.TriangleStrip:
                    GL.DrawElements(PrimitiveType.TriangleStrip, indicies.Length, DrawElementsType.UnsignedInt, 0);
                    break;
                case MeshElementType.PolygonList:
                    GL.DrawElements(PrimitiveType.Polygon, indicies.Length, DrawElementsType.UnsignedInt, 0);
                    break;
            }
        }

        public static void UploadQuadMesh()
        {
            var mesh = new Mesh();
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

            GL.GenVertexArrays(1, out vao);
            GL.BindVertexArray(vao);

            GL.GenBuffers(1, out vbo);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(verticies.Length * VertexFormat.Size), verticies, BufferUsageHint.StaticDraw);

            GL.GenBuffers(1, out ibo);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indicies.Length * sizeof(uint)), indicies, BufferUsageHint.StaticDraw);

            SetupVertexFormatAttributes();

            QuadMeshId = vao;
        }

        private static void SetupVertexFormatAttributes()
        {
            // Attributes for VertexFormat.Position
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, VertexFormat.Size, 0);

            // Attributes for VertexFormat.TexCoords
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, VertexFormat.Size, 12);

            // Attributes for VertexFormat.Normal
            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, VertexFormat.Size, 20);

            // Attributes for VertexFormat.Tangent
            GL.EnableVertexAttribArray(3);
            GL.VertexAttribPointer(3, 3, VertexAttribPointerType.Float, false, VertexFormat.Size, 32);

            // Attributes for VertexFormat.Bitangent
            GL.EnableVertexAttribArray(4);
            GL.VertexAttribPointer(4, 3, VertexAttribPointerType.Float, false, VertexFormat.Size, 44);
        }

        private static void SetupMatrixUniform()
        {
            if (MatriciesUniformHandle == default(int))
                GL.GenBuffers(1, out MatriciesUniformHandle);

            GL.BindBuffer(BufferTarget.UniformBuffer, MatriciesUniformHandle);

            GL.BufferData(BufferTarget.UniformBuffer, GlobalUniform.Size, ref MatriciesUniform, BufferUsageHint.DynamicDraw);

            GL.BindBufferBase(BufferRangeTarget.UniformBuffer, 0, MatriciesUniformHandle);
            GL.BindBuffer(BufferTarget.UniformBuffer, 0);
        }

        public static void DebugCallbackF(DebugSource source, DebugType type, int id, DebugSeverity severity, int length, IntPtr message, IntPtr userParam)
        {
            if (severity == DebugSeverity.DebugSeverityNotification)
                return;

            string msg = Marshal.PtrToStringAnsi(message, length);
            Console.WriteLine(msg);
        }
    }
}
