using System;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using Avalonia;
using Avalonia.Media.Imaging;
using OpenH2.Core.Tags;
using OpenH2.Foundation;
using OpenH2.Rendering.OpenGL;
using OpenH2.Rendering.Shaders;
using OpenH2.ScenarioExplorer.ViewModels;
using Silk.NET.OpenGL;
using Shader = OpenH2.Rendering.Shaders.Shader;

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

        private unsafe static TagPreviewViewModel GetBitmPreview(BitmapTag bitm)
        {
            var preview = new TagPreviewViewModel();

            preview.AddItem("bitmap", bitm, GetBitmapPreview);

            return preview;

            object GetBitmapPreview(BitmapTag bitm)
            {
                // HACK: hard coding texture 0, needs texture binder rewrite to support here
                if (bitm.TextureInfos[0].Width == 0 || bitm.TextureInfos[0].Height == 0)
                    return null;

                var host = new OpenGLHost();
                host.CreateWindow(new Vector2(bitm.TextureInfos[0].Width, bitm.TextureInfos[0].Height), hidden: true);
                var window = host.GetWindow();

                var textureBinder = new OpenGLTextureBinder(host);
                gl = GL.GetApi(window);

                gl.Enable(EnableCap.DebugOutput);
                gl.DebugMessageCallback(callback, (IntPtr.Zero));

                gl.Enable(EnableCap.DepthTest);
                gl.Enable(EnableCap.CullFace);
                gl.Enable(EnableCap.Blend);
                gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

                var meshId = UploadQuadMesh();
                var matriciesUniform = new GlobalUniform()
                {
                    ProjectionMatrix = Matrix4x4.CreateOrthographic(2f, 2f, 0, 10),
                    ViewMatrix = Matrix4x4.Identity,
                    ViewPosition = Vector3.Zero
                };

                var shader = OpenGLShaderCompiler.CreateShader(Shader.TextureViewer);

                var handle = (uint)textureBinder.GetOrBind(bitm, out var _);
                gl.ActiveTexture(TextureUnit.Texture0);
                gl.BindTexture(GLEnum.Texture2D, handle);

                gl.ClearColor(0f, 0f, 0f, 1f);
                gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                gl.UseProgram(shader);

                gl.GenBuffers(1, out uint MatriciesUniformHandle);

                gl.BindBuffer(GLEnum.UniformBuffer, MatriciesUniformHandle);

                gl.BufferData(GLEnum.UniformBuffer, (uint)GlobalUniform.Size, matriciesUniform, GLEnum.DynamicDraw);

                gl.BindBufferBase(GLEnum.UniformBuffer, 0, MatriciesUniformHandle);
                gl.BindBuffer(GLEnum.UniformBuffer, 0);

                gl.BindVertexArray(meshId);

                gl.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, (void*)0);

                gl.Flush();

                var bmp = new WriteableBitmap(new PixelSize(bitm.TextureInfos[0].Width, bitm.TextureInfos[0].Height), new Avalonia.Vector(72, 72), Avalonia.Platform.PixelFormat.Rgba8888);
                using (var buf = bmp.Lock())
                {
                    gl.ReadPixels(0, 0, (uint)bitm.TextureInfos[0].Width, (uint)bitm.TextureInfos[0].Height, GLEnum.Rgba, GLEnum.UnsignedByte, (void*)buf.Address);
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

            // Install from https://archive.org/details/dxsdk_nov08
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
        private static GL gl;

        public static void DebugCallbackF(GLEnum source, GLEnum type, int id, GLEnum severity, int length, IntPtr message, IntPtr userParam)
        {
            if (severity == GLEnum.DebugSeverityNotification)
                return;

            string msg = Marshal.PtrToStringAnsi(message, length);
            Console.WriteLine(msg);
        }

        public static unsafe uint UploadQuadMesh()
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


            gl.GenVertexArrays(1, out uint vao);
            gl.BindVertexArray(vao);

            gl.GenBuffers(1, out uint vbo);
            gl.BindBuffer(GLEnum.ArrayBuffer, vbo);
            gl.BufferData<VertexFormat>(GLEnum.ArrayBuffer, (nuint)(verticies.Length * VertexFormat.Size), verticies, GLEnum.StaticDraw);

            gl.GenBuffers(1, out uint ibo);
            gl.BindBuffer(GLEnum.ElementArrayBuffer, ibo);
            gl.BufferData<int>(GLEnum.ElementArrayBuffer, (nuint)(indicies.Length * sizeof(uint)), indicies, GLEnum.StaticDraw);

            // Attributes for VertexFormat.Position
            gl.EnableVertexAttribArray(0);
            gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, (uint)VertexFormat.Size, (void*)0);

            // Attributes for VertexFormat.TexCoords
            gl.EnableVertexAttribArray(1);
            gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, (uint)VertexFormat.Size, (void*)12);

            // Attributes for VertexFormat.Normal
            gl.EnableVertexAttribArray(2);
            gl.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, (uint)VertexFormat.Size, (void*)20);

            // Attributes for VertexFormat.Tangent
            gl.EnableVertexAttribArray(3);
            gl.VertexAttribPointer(3, 3, VertexAttribPointerType.Float, false, (uint)VertexFormat.Size, (void*)32);

            // Attributes for VertexFormat.Bitangent
            gl.EnableVertexAttribArray(4);
            gl.VertexAttribPointer(4, 3, VertexAttribPointerType.Float, false, (uint)VertexFormat.Size, (void*)44);

            return vao;
        }


#endregion
    }
}
