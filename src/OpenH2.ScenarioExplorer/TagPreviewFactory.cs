using OpenH2.Core.Tags;
using OpenH2.ScenarioExplorer.ViewModels;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using OpenTK.Graphics.OpenGL;
using OpenH2.Rendering.OpenGL;
using OpenH2.Rendering.Shaders;
using System.Numerics;
using OpenH2.Foundation;
using PixelFormat = OpenToolkit.Graphics.OpenGL.PixelFormat;
using System.Runtime.InteropServices;
using Avalonia.Media.Imaging;
using Avalonia;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace OpenH2.ScenarioExplorer
{
    public class TagPreviewFactory
    {
        public static TagPreviewViewModel GetPreview(BaseTag tag)
        {
            switch(tag)
            {
                case ShaderPassTag spas:
                    return GetSpasPreview(spas);

                case BitmapTag bitm:
                    return GetBitmPreview(bitm);

                case VertexShaderTag vert:
                    return GetVertexPreview(vert);

                default:
                    return null;
            }
        }

        private static TagPreviewViewModel GetBitmPreview(BitmapTag bitm)
        {
            var preview = new TagPreviewViewModel();

            preview.AddItem("bitmap", bitm, GetBitmapPreview);

            return preview;

            object GetBitmapPreview(BitmapTag bitm)
            {
                // HACK: hard coding texture 0, needs texture binder rewrite to support here
                if (bitm.TextureInfos[0].Width == 0 || bitm.TextureInfos[0].Height == 0)
                    return null;

                var textureBinder = new OpenGLTextureBinder();

                var settings = new GameWindowSettings();

                var nsettings = new NativeWindowSettings()
                {
                    API = ContextAPI.OpenGL,
                    AutoLoadBindings = true,
                    Size = new OpenToolkit.Mathematics.Vector2i(bitm.TextureInfos[0].Width, bitm.TextureInfos[0].Height),
                    Title = "OpenH2",
                    Flags = ContextFlags.Debug | ContextFlags.Offscreen,
                    APIVersion = new Version(4, 0)
                };

                var window = new GameWindow(settings, nsettings);

                window.IsVisible = false;
                window.MakeCurrent();

                GL.Enable(EnableCap.DebugOutput);
                GL.DebugMessageCallback(callback, (IntPtr.Zero));

                GL.Enable(EnableCap.DepthTest);
                GL.Enable(EnableCap.CullFace);
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

                var meshId = UploadQuadMesh();
                var matriciesUniform = new GlobalUniform()
                {
                    ProjectionMatrix = Matrix4x4.CreateOrthographic(2f, 2f, 0, 10),
                    ViewMatrix = Matrix4x4.Identity,
                    ViewPosition = Vector3.Zero
                };

                var shader = ShaderCompiler.CreateShader(Shader.TextureViewer);

                var handle = textureBinder.GetOrBind(bitm, out var _);
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture2D, handle);

                GL.ClearColor(0f, 0f, 0f, 1f);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                GL.UseProgram(shader);

                GL.GenBuffers(1, out uint MatriciesUniformHandle);

                GL.BindBuffer(BufferTarget.UniformBuffer, MatriciesUniformHandle);

                GL.BufferData(BufferTarget.UniformBuffer, GlobalUniform.Size, ref matriciesUniform, BufferUsageHint.DynamicDraw);

                GL.BindBufferBase(BufferRangeTarget.UniformBuffer, 0, MatriciesUniformHandle);
                GL.BindBuffer(BufferTarget.UniformBuffer, 0);

                GL.BindVertexArray(meshId);

                GL.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, 0);

                GL.Flush();

                var bmp = new WriteableBitmap(new PixelSize(bitm.TextureInfos[0].Width, bitm.TextureInfos[0].Height), new Avalonia.Vector(72, 72), Avalonia.Platform.PixelFormat.Rgba8888);
                using (var buf = bmp.Lock())
                {
                    GL.ReadPixels(0, 0, bitm.TextureInfos[0].Width, bitm.TextureInfos[0].Height, PixelFormat.Rgba, PixelType.UnsignedByte, buf.Address);
                }

                window.Close();
                window.Dispose();

                return bmp;
            }
        }

        private static TagPreviewViewModel GetVertexPreview(VertexShaderTag vert)
        {
            var preview = new TagPreviewViewModel();

            var shaderCount = 0;

            foreach (var shader in vert.Shaders)
            {

                if (shader.ShaderData?.Length > 0)
                    preview.AddItem($"Shader {shaderCount++}", shader.ShaderData, GetShaderPreview);
            }

            return preview;
        }

        private static TagPreviewViewModel GetSpasPreview(ShaderPassTag spas)
        {
            var preview = new TagPreviewViewModel();

            var shaderCount = 0;

            foreach (var wrapper in spas.Wrapper1s)
            {
                foreach (var wrapper2 in wrapper.Wrapper2s)
                {
                    foreach (var shader in wrapper2.ShaderReferenceGroups1)
                    {
                        if (shader.ShaderData1?.Length > 0)
                            preview.AddItem($"Shader {shaderCount++}", shader.ShaderData1, GetShaderPreview);

                        if (shader.ShaderData2?.Length > 0)
                            preview.AddItem($"Shader {shaderCount++}", shader.ShaderData2, GetShaderPreview);

                        if (shader.ShaderData3?.Length > 0)
                            preview.AddItem($"Shader {shaderCount++}", shader.ShaderData3, GetShaderPreview);
                    }
                }
            }

            return preview;
        }

#region Shader Helpers
        private static object GetShaderPreview(byte[] shaderData)
        {
            var output = new StringBuilder();

            var utilsPath = Path.Combine(Environment.GetEnvironmentVariable("DXSDK_DIR", EnvironmentVariableTarget.Machine), "Utilities\\bin\\x64");

            var path = Path.Combine(utilsPath, "psa.exe");

            var shadIn = Path.GetTempFileName();
            File.WriteAllBytes(shadIn, shaderData);

            var shadOut = Path.GetTempFileName();
            var shadReflect = Path.GetTempFileName();

            var start = new ProcessStartInfo(path, $"/nologo \"{shadIn}\" /Fc \"{shadOut}\"");
            start.RedirectStandardOutput = true;
            start.RedirectStandardError = true;
            start.UseShellExecute = false;
            start.CreateNoWindow = true;

            var proc = new Process();
            proc.StartInfo = start;
            proc.OutputDataReceived += (s, e) => output.AppendLine(e.Data);
            proc.ErrorDataReceived += (s, e) => output.AppendLine(e.Data);

            proc.Start();
            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();

            if (proc.WaitForExit(1000))
            {
                var sb = new StringBuilder();
                var shadAsm = File.ReadAllText(shadOut);
                sb.AppendLine(shadAsm);
                sb.AppendLine(ShaderCodeGeneration.TranslateAsmShaderToPseudocode(shadAsm));
                sb.Append(output);

                output = sb;
            }
            else
            {
                proc.Kill();
                output.Insert(0, "Error decompiling shader data\r\n");
            }

            File.Delete(shadIn);
            File.Delete(shadOut);
            File.Delete(shadReflect);

            return output.ToString().Replace("\t", "    ");
        }

#endregion


#region Render Helpers

        private static DebugProc callback = DebugCallbackF;

        public static void DebugCallbackF(DebugSource source, DebugType type, int id, DebugSeverity severity, int length, IntPtr message, IntPtr userParam)
        {
            if (severity == DebugSeverity.DebugSeverityNotification)
                return;

            string msg = Marshal.PtrToStringAnsi(message, length);
            Console.WriteLine(msg);
        }

        public static uint UploadQuadMesh()
        {
            var mesh = new Mesh<BitmapTag>();
            mesh.Verticies = new VertexFormat[]
            {
                new VertexFormat(new Vector3(-1,-1,1), new Vector2(0,1), Vector3.Zero),
                new VertexFormat(new Vector3(-1,1,1), new Vector2(0,0), Vector3.Zero),
                new VertexFormat(new Vector3(1,1,1), new Vector2(1,0), Vector3.Zero),
                new VertexFormat(new Vector3(1,-1,1), new Vector2(1,1), Vector3.Zero),
            };

            mesh.Indicies = new int[] { 2, 1, 0, 3, 2, 0 };
            mesh.ElementType = MeshElementType.TriangleList;

            var verticies = mesh.Verticies;
            var indicies = mesh.Indicies;


            GL.GenVertexArrays(1, out uint vao);
            GL.BindVertexArray(vao);

            GL.GenBuffers(1, out uint vbo);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(verticies.Length * VertexFormat.Size), verticies, BufferUsageHint.StaticDraw);

            GL.GenBuffers(1, out uint ibo);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indicies.Length * sizeof(uint)), indicies, BufferUsageHint.StaticDraw);

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

            return vao;
        }


#endregion
    }
}
