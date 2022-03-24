using OpenH2.Core.Tags;
using OpenH2.Foundation;
using OpenH2.Rendering.Abstractions;
using OpenH2.Rendering.Shaders;
using OpenH2.Rendering.Shaders.Generic;
using OpenH2.Rendering.Shaders.ShadowMapping;
using OpenH2.Rendering.Shaders.Skybox;
using OpenH2.Rendering.Shaders.Wireframe;
using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Shader = OpenH2.Rendering.Shaders.Shader;

namespace OpenH2.Rendering.OpenGL
{
    public partial class OpenGLGraphicsAdapter : IGraphicsAdapter
    {
        private const int ShadowMapSize = 4096;
        private const int ShadowCascadeCount = 4;

        private readonly OpenGLHost host;
        private GL gl => host.gl;

        // Uniform index used in shaders
        private class UniformIndices
        {
            public const int Global = 0;
            public const int Shader = 1;
            public const int Transform = 2;
        }

        private Dictionary<IMaterial<BitmapTag>, MaterialBindings> boundMaterials = new Dictionary<IMaterial<BitmapTag>, MaterialBindings>();
        private ITextureBinder textureBinder;

        private Action?[] shaderBeginActions = new Action?[(int)Shader.MAX_VALUE];
        private Action?[] shaderEndActions = new Action?[(int)Shader.MAX_VALUE];
        private Action<Action<DrawCommand[]>, DrawCommand[]>?[] shaderDrawActions = new Action<Action<DrawCommand[]>, DrawCommand[]>?[(int)Shader.MAX_VALUE];

        private Shader activeShader;
        private uint[] shaderHandles = new uint[(int)Shader.MAX_VALUE];

        private uint GlobalUniformHandle;
        private GlobalUniform GlobalUniform;

        private uint LightingUniformHandle;
        private LightingUniform LightingUniform;

        private uint TransformUniformHandle;

        private (uint depthFbo, uint depthMaps) shadowBuffers;

        public OpenGLGraphicsAdapter(OpenGLHost host)
        {
            this.host = host;
            OpenGLShaderCompiler.UseHost(host);
            this.textureBinder = new OpenGLTextureBinder(host);


            this.shaderBeginActions[(int)Shader.Skybox] = () => {
                gl.Disable(EnableCap.DepthTest);
            };

            this.shaderEndActions[(int)Shader.Skybox] = () => {
                gl.Enable(EnableCap.DepthTest);
            };

            this.shaderBeginActions[(int)Shader.ShadowMapping] = () => {
                gl.BindFramebuffer((GLEnum)FramebufferTarget.Framebuffer, this.shadowBuffers.depthFbo);
                // Tutorial had Texture2DArray instead of DepthAttachment here, but that doesn't really make sense
                gl.FramebufferTexture((GLEnum)FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, this.shadowBuffers.depthMaps, 0);
                gl.Viewport(0, 0, ShadowMapSize, ShadowMapSize);
                gl.Clear(ClearBufferMask.DepthBufferBit);
            };

            this.shaderEndActions[(int)Shader.ShadowMapping] = () => {
                var size = this.host.ViewportSize;
                gl.Viewport(0, 0, (uint)size.X, (uint)size.Y);
                gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                gl.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);
            };

            this.shaderBeginActions[(int)Shader.Generic] = () =>
            {
                // Bind shadow maps
                gl.ActiveTexture(TextureUnit.Texture16);
                gl.BindTexture(TextureTarget.Texture2DArray, this.shadowBuffers.depthMaps);
                gl.Uniform1(gl.GetUniformLocation(shaderHandles[(int)activeShader], "shadowMap"), 16);
                //gl.BindSampler(16, 16);
            };
        }

        public void BeginFrame(GlobalUniform global)
        {
            if(this.shadowBuffers == default)
            {
                this.InitializeShadowMapBuffers();
            }

            GlobalUniform = global;
            SetupGlobalUniform();

            LightingUniform = new LightingUniform() { PointLights = Array.Empty<PointLightUniform>() };
        }

        public void UseShader(Shader shader)
        {
            if(activeShader == shader)
            {
                return;
            }

            shaderEndActions[(int)activeShader]?.Invoke();

            var handle = shaderHandles[(int)shader];
            if (handle == 0)
            {
                handle = OpenGLShaderCompiler.CreateShader(shader);
                shaderHandles[(int)shader] = handle;
            }

            gl.UseProgram(handle);

            activeShader = shader;

            shaderBeginActions[(int)shader]?.Invoke();
        }

        public void SetSunLight(Vector3 sunDirection)
        {
            //LightingUniform.SunDirection = new Vector4(sunDirection, 0f);
        }

        public void AddLight(PointLight light)
        {
            var newLights = new List<PointLightUniform>(LightingUniform.PointLights);
            newLights.Add(new PointLightUniform(light));

            LightingUniform.PointLights = newLights.ToArray();
        }

        public int UploadModel(Model<BitmapTag> model, out DrawCommand[] meshCommands)
        {
            uint vao, vbo, ibo;

            gl.GenVertexArrays(1, out vao);
            gl.BindVertexArray(vao);

            var vertCount = model.Meshes.Sum(m => m.Verticies.Length);
            var indxCount = model.Meshes.Sum(m => m.Indicies.Length);

            meshCommands = new DrawCommand[model.Meshes.Length];
            var vertices = new VertexFormat[vertCount];
            var indices = new int[indxCount];

            var currentVert = 0;
            var currentIndx = 0;

            for(var i = 0; i < model.Meshes.Length; i++)
            {
                var mesh = model.Meshes[i];

                var command = new DrawCommand(mesh)
                {
                    VaoHandle = (int)vao,
                    IndexBase = currentIndx,
                    VertexBase = currentVert,
                    ColorChangeData = model.ColorChangeData
                };

                Array.Copy(mesh.Verticies, 0, vertices, currentVert, mesh.Verticies.Length);
                currentVert += mesh.Verticies.Length;

                Array.Copy(mesh.Indicies, 0, indices, currentIndx, mesh.Indicies.Length);
                currentIndx += mesh.Indicies.Length;

                meshCommands[i] = command;
            }

            gl.GenBuffers(1, out vbo);
            gl.BindBuffer(GLEnum.ArrayBuffer, vbo);
            gl.BufferData<VertexFormat>(GLEnum.ArrayBuffer, (nuint)(vertices.Length * VertexFormat.Size), vertices, GLEnum.StaticDraw);

            gl.GenBuffers(1, out ibo);
            gl.BindBuffer(GLEnum.ElementArrayBuffer, ibo);
            gl.BufferData<int>(GLEnum.ElementArrayBuffer, (nuint)(indices.Length * sizeof(int)), indices, GLEnum.StaticDraw);

            SetupVertexFormatAttributes();

            return (int)vao;
        }

        public void EndFrame()
        {
        }

        public MaterialBindings SetupTextures(IMaterial<BitmapTag> material)
        {
            if (boundMaterials.TryGetValue(material, out var bindings))
            {
                return bindings;
            }

            bindings = new MaterialBindings();

            if (material.DiffuseMap != null)
            {
                textureBinder.GetOrBind(material.DiffuseMap, out var diffuseHandle);
                bindings.DiffuseHandle = diffuseHandle;
            }

            if (material.DetailMap1 != null)
            {
                textureBinder.GetOrBind(material.DetailMap1, out var handle);
                bindings.Detail1Handle = handle;
            }

            if (material.DetailMap2 != null)
            {
                textureBinder.GetOrBind(material.DetailMap2, out var handle);
                bindings.Detail2Handle = handle;
            }

            if (material.ColorChangeMask != null)
            {
                textureBinder.GetOrBind(material.ColorChangeMask, out var handle);
                bindings.ColorChangeHandle = handle;
            }

            if (material.AlphaMap != null)
            {
                textureBinder.GetOrBind(material.AlphaMap, out var alphaHandle);
                bindings.AlphaHandle = alphaHandle;
            }

            if (material.EmissiveMap != null)
            {
                textureBinder.GetOrBind(material.EmissiveMap, out var emissiveHandle);
                bindings.EmissiveHandle = emissiveHandle;
            }

            if (material.NormalMap != null)
            {
                textureBinder.GetOrBind(material.NormalMap, out var handle);
                bindings.NormalHandle = handle;
            }

            boundMaterials.Add(material, bindings);

            return bindings;
        }

        public void UseTransform(Matrix4x4 transform)
        {
            var success = Matrix4x4.Invert(transform, out var inverted);
            Debug.Assert(success);

            SetupTransformUniform(new TransformUniform(transform, inverted));
        }

        public unsafe void DrawMeshes(DrawCommand[] commands)
        {
            for (var i = 0; i < commands.Length; i++)
            {
                ref DrawCommand command = ref commands[i];

                SetupLighting();

                BindOrCreateShaderUniform(ref command);
                BindVao(ref command);

                var primitiveType = command.ElementType switch
                {
                    MeshElementType.TriangleList => PrimitiveType.Triangles,
                    MeshElementType.TriangleStrip => PrimitiveType.TriangleStrip,
                    MeshElementType.TriangleStripDecal => PrimitiveType.TriangleStrip,
                    MeshElementType.Point => PrimitiveType.Points,
                    _ => PrimitiveType.Triangles
                };

                var indexBaseOffset = command.IndexBase * sizeof(int);

                gl.DrawElementsBaseVertex((GLEnum)primitiveType,
                    (uint)command.IndiciesCount,
                    GLEnum.UnsignedInt,
                    (void*)indexBaseOffset,
                    command.VertexBase);
            }
        }

        public void InitializeShadowMapBuffers()
        {
            var error1 = gl.GetError();
            if (error1 != GLEnum.NoError)
            {
                Console.WriteLine("-- Error {0} occured {1}", error1, "before shadow map bufs");
            }

            
            gl.GenFramebuffers(1, out uint depthFbo);
            gl.GenTextures(1, out uint depthMap);

            gl.BindTexture(TextureTarget.Texture2DArray, depthMap);
            gl.TexImage3D<float>(TextureTarget.Texture2DArray, 0, (int)GLEnum.DepthComponent32f,
                ShadowMapSize, ShadowMapSize, ShadowCascadeCount + 1, 0, PixelFormat.DepthComponent, PixelType.Float, new float[0]);

            gl.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMinFilter, (float)TextureMinFilter.Nearest);
            gl.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMagFilter, (float)TextureMagFilter.Nearest);
            gl.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
            gl.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);
            var borderColor = new[] { 1.0f, 1.0f, 1.0f, 1.0f };
            gl.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureBorderColor, borderColor);

            gl.BindFramebuffer(FramebufferTarget.Framebuffer, depthFbo);
            gl.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, depthMap, 0);
            gl.DrawBuffer(DrawBufferMode.None);
            gl.ReadBuffer(ReadBufferMode.None);
            gl.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            this.shadowBuffers = (depthFbo, depthMap);
            

            error1 = gl.GetError();
            if (error1 != GLEnum.NoError)
            {
                Console.WriteLine("-- Error {0} occured {1}", error1, "during shadow map bufs");
            }
        }

        private Dictionary<IMaterial<BitmapTag>, int[]> MaterialUniforms = new Dictionary<IMaterial<BitmapTag>, int[]>();

        private int currentlyBoundShaderUniform = -1;
        private unsafe void BindOrCreateShaderUniform(ref DrawCommand command)
        {
            // If the uniform was already buffered, we'll just reuse that buffered uniform
            // Currently these material uniforms never change at runtime - if this changes
            // there will have to be some sort of invalidation to ensure they're updated
            if (command.ShaderUniformHandle[(int)activeShader] != -1)
            {
                if(command.ShaderUniformHandle[(int)activeShader] != currentlyBoundShaderUniform)
                {
                    gl.BindBufferBase(GLEnum.UniformBuffer, UniformIndices.Shader, (uint)command.ShaderUniformHandle[(int)activeShader]);
                }
            }
            else
            {
                if (MaterialUniforms.TryGetValue(command.Mesh.Material, out var uniforms) == false)
                {
                    uniforms = new int[(int)Shader.MAX_VALUE];
                    MaterialUniforms[command.Mesh.Material] = uniforms;
                }

                var bindings = SetupTextures(command.Mesh.Material);
                command.ShaderUniformHandle[(int)activeShader] = (int)GenerateShaderUniform(command, bindings);
                uniforms[(int)activeShader] = command.ShaderUniformHandle[(int)activeShader];
            }
        }

        private uint GenerateShaderUniform(DrawCommand command, MaterialBindings bindings)
        {
            var mesh = command.Mesh;
            var existingUniformHandle = 0u;

            switch (activeShader)
            {
                case Shader.Skybox:
                    BindAndBufferShaderUniform(
                        activeShader,
                        new SkyboxUniform(mesh.Material, bindings),
                        SkyboxUniform.Size,
                        out existingUniformHandle);
                    break;
                case Shader.Generic:
                    BindAndBufferShaderUniform(
                        activeShader,
                        new GenericUniform(mesh, command.ColorChangeData, bindings),
                        GenericUniform.Size,
                        out existingUniformHandle);

                    break;
                case Shader.Wireframe:
                    BindAndBufferShaderUniform(
                        activeShader,
                        new WireframeUniform(mesh.Material),
                        WireframeUniform.Size,
                        out existingUniformHandle);
                    break;
                case Shader.ShadowMapping:
                    BindAndBufferShaderUniform(
                        activeShader,
                        new ShadowMappingUniform(),
                        ShadowMappingUniform.Size,
                        out existingUniformHandle);
                    break;
                case Shader.Pointviz:
                case Shader.TextureViewer:
                    break;
            }

            return existingUniformHandle;
        }

        private int currentVao = -1;
        private void BindVao(ref DrawCommand command)
        {
            Debug.Assert(command.VaoHandle != -1);

            if (currentVao != command.VaoHandle)
            {
                gl.BindVertexArray((uint)command.VaoHandle);
                currentVao = command.VaoHandle;
            }
        }

        private unsafe void SetupVertexFormatAttributes()
        {
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
        }

        private void SetupGlobalUniform()
        {
            if (GlobalUniformHandle == default(int))
            {
                gl.GenBuffers(1, out GlobalUniformHandle);
                gl.BindBuffer(GLEnum.UniformBuffer, GlobalUniformHandle);
                gl.BufferData(GLEnum.UniformBuffer, (uint)GlobalUniform.Size, IntPtr.Zero, GLEnum.DynamicDraw);
            }
            else
            {
                gl.BindBuffer(GLEnum.UniformBuffer, GlobalUniformHandle);
            }

            gl.BufferSubData(GLEnum.UniformBuffer, IntPtr.Zero, (uint)GlobalUniform.Size, in GlobalUniform);

            gl.BindBufferBase(GLEnum.UniformBuffer, UniformIndices.Global, GlobalUniformHandle);
            gl.BindBuffer(GLEnum.UniformBuffer, 0);
        }

        private void BindAndBufferShaderUniform<T>(Shader shader, T uniform, int size, out uint handle) where T : unmanaged
        {
            gl.GenBuffers(1, out handle);
            gl.BindBuffer(GLEnum.UniformBuffer, handle);
            gl.BufferData(GLEnum.UniformBuffer, (nuint)size, in uniform, GLEnum.DynamicDraw);

            gl.BindBufferBase(GLEnum.UniformBuffer, UniformIndices.Shader, handle);
            gl.BindBuffer(GLEnum.UniformBuffer, 0);
        }

        private void SetupTransformUniform(TransformUniform transform)
        {
            if (TransformUniformHandle == default(int))
            {
                gl.GenBuffers(1, out TransformUniformHandle);
                gl.BindBuffer(GLEnum.UniformBuffer, TransformUniformHandle);
                gl.BufferData(GLEnum.UniformBuffer, (uint)GlobalUniform.Size, IntPtr.Zero, GLEnum.DynamicDraw);
            }
            else
            {
                gl.BindBuffer(GLEnum.UniformBuffer, TransformUniformHandle);
            }

            gl.BufferSubData(GLEnum.UniformBuffer, IntPtr.Zero, (uint)TransformUniform.Size, in transform);

            gl.BindBufferBase(GLEnum.UniformBuffer, UniformIndices.Transform, TransformUniformHandle);
            gl.BindBuffer(GLEnum.UniformBuffer, 0);
        }

        public void SetupLighting()
        {
            if (LightingUniformHandle == default(int))
            {
                gl.GenBuffers(1, out LightingUniformHandle);
                gl.BindBuffer(GLEnum.UniformBuffer, LightingUniformHandle);
                gl.BufferData(GLEnum.UniformBuffer, 320, IntPtr.Zero, GLEnum.DynamicDraw);
            }
            else
            {
                gl.BindBuffer(GLEnum.UniformBuffer, LightingUniformHandle);
            }

            //gl.BufferSubData<byte>(GLEnum.UniformBuffer, IntPtr.Zero, 320, in LightingUniform.PointLights[0]);

            gl.BindBufferBase(GLEnum.UniformBuffer, 3, LightingUniformHandle);
            gl.BindBuffer(GLEnum.UniformBuffer, 0);
        }
    }
}
