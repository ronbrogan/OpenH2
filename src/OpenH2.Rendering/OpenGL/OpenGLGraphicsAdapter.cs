using OpenH2.Core.Tags;
using OpenH2.Foundation;
using OpenH2.Rendering.Abstractions;
using OpenH2.Rendering.Shaders;
using OpenH2.Rendering.Shaders.Generic;
using OpenH2.Rendering.Shaders.Skybox;
using OpenH2.Rendering.Shaders.Wireframe;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace OpenH2.Rendering.OpenGL
{
    public class OpenGLGraphicsAdapter : IGraphicsAdapter
    {
        private Dictionary<Mesh<BitmapTag>, uint> meshLookup = new Dictionary<Mesh<BitmapTag>, uint>();
        private Dictionary<IMaterial<BitmapTag>, MaterialBindings> boundMaterials = new Dictionary<IMaterial<BitmapTag>, MaterialBindings>();
        private ITextureBinder textureBinder = new OpenGLTextureBinder();

        private Shader activeShader;
        private Dictionary<Shader, int> shaderHandles = new Dictionary<Shader, int>();
        private Dictionary<Shader, int> uniformHandles = new Dictionary<Shader, int>();

        private int GlobalUniformHandle;
        private GlobalUniform GlobalUniform;

        private int LightingUniformHandle;
        private LightingUniform LightingUniform;

        public OpenGLGraphicsAdapter()
        {
        }

        public void BeginFrame(GlobalUniform global)
        {
            GlobalUniform = global;
            SetupGlobalUniform();

            LightingUniform = new LightingUniform() { PointLights = new PointLightUniform[0] };
        }

        public void UseShader(Shader shader)
        {
            if (shaderHandles.TryGetValue(shader, out var handle) == false)
            {
                handle = ShaderCompiler.CreateShader(shader);
                shaderHandles[shader] = handle;
            }

            GL.UseProgram(handle);

            activeShader = shader;

            if(shaderBeginActions.TryGetValue(shader, out var action))
            {
                action();
            }
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

        public uint UploadMesh(Mesh<BitmapTag> mesh)
        {
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

            meshLookup[mesh] = vao;
            return vao;
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
                textureBinder.Bind(material.DiffuseMap, out var diffuseHandle);
                bindings.DiffuseHandle = diffuseHandle;
            }

            if (material.DetailMap1 != null)
            {
                textureBinder.Bind(material.DetailMap1, out var handle);
                bindings.Detail1Handle = handle;
            }

            if (material.DetailMap2 != null)
            {
                textureBinder.Bind(material.DetailMap2, out var handle);
                bindings.Detail2Handle = handle;
            }

            if (material.AlphaMap != null)
            {
                textureBinder.Bind(material.AlphaMap, out var alphaHandle);
                bindings.AlphaHandle = alphaHandle;
            }

            if (material.EmissiveMap != null)
            {
                textureBinder.Bind(material.EmissiveMap, out var emissiveHandle);
                bindings.EmissiveHandle = emissiveHandle;
            }

            if (material.NormalMap != null)
            {
                textureBinder.Bind(material.NormalMap, out var handle);
                bindings.NormalHandle = handle;
            }

            boundMaterials.Add(material, bindings);

            return bindings;
        }

        public void SetupLighting()
        {
            if (LightingUniformHandle == default(int))
            {
                GL.GenBuffers(1, out LightingUniformHandle);
                GL.BindBuffer(BufferTarget.UniformBuffer, LightingUniformHandle);
                GL.BufferData(BufferTarget.UniformBuffer, 320, IntPtr.Zero, BufferUsageHint.DynamicDraw);
            }
            else
            {
                GL.BindBuffer(BufferTarget.UniformBuffer, LightingUniformHandle);
            }

            GL.BufferSubData(BufferTarget.UniformBuffer, IntPtr.Zero, 320, LightingUniform.PointLights);

            GL.BindBufferBase(BufferRangeTarget.UniformBuffer, 2, LightingUniformHandle);
            GL.BindBuffer(BufferTarget.UniformBuffer, 0);
        }

        // PERF: sort calls by material and vao and deduplicate GL calls 
        public void DrawMesh(Mesh<BitmapTag> mesh, Matrix4x4 transform)
        {
            SetupLighting();

            var bindings = SetupTextures(mesh.Material);

            CreateAndBindShaderUniform(mesh, bindings, transform);

            BindMesh(mesh);

            var type = mesh.ElementType;
            var indicies = mesh.Indicies;

            switch (type)
            {
                case MeshElementType.TriangleList:
                    GL.DrawElements(PrimitiveType.Triangles, indicies.Length, DrawElementsType.UnsignedInt, 0);
                    break;
                case MeshElementType.TriangleStrip:
                case MeshElementType.TriangleStripDecal:
                    GL.DrawElements(PrimitiveType.TriangleStrip, indicies.Length, DrawElementsType.UnsignedInt, 0);
                    break;
                case MeshElementType.Point:
                    GL.DrawElements(PrimitiveType.Points, indicies.Length, DrawElementsType.UnsignedInt, 0);
                    break;
                default:
                    GL.DrawElements(PrimitiveType.Triangles, indicies.Length, DrawElementsType.UnsignedInt, 0);
                    break;
            }
        }

        private void CreateAndBindShaderUniform(Mesh<BitmapTag> mesh, MaterialBindings bindings, Matrix4x4 transform)
        {
            if (Matrix4x4.Invert(transform, out var inverted) == false)
            {
                Console.WriteLine("Couldn't invert model matrix: " + mesh.Note);
                return;
            }

            switch (activeShader)
            {
                case Shader.Skybox:
                    SetupGenericUniform(
                        activeShader,
                        new SkyboxUniform(mesh.Material, bindings, transform, inverted),
                        SkyboxUniform.Size);
                    break;
                case Shader.Generic:
                    SetupGenericUniform(
                        activeShader, 
                        new GenericUniform(mesh.Material, bindings, transform, inverted), 
                        GenericUniform.Size);
                    break;
                case Shader.Wireframe:
                    SetupGenericUniform(
                        activeShader,
                        new WireframeUniform(mesh.Material, bindings, transform, inverted),
                        WireframeUniform.Size);
                    break;
                case Shader.Pointviz:
                    SetupGenericUniform(
                        activeShader,
                        new PointvizUniform(mesh.Material, bindings, transform, inverted),
                        PointvizUniform.Size);
                    break;
                case Shader.TextureViewer:
                    break;
            }
        }

        private void BindMesh(Mesh<BitmapTag> mesh)
        {
            if (mesh.Dirty || meshLookup.TryGetValue(mesh, out var vaoId) == false)
            {
                vaoId = UploadMesh(mesh);
            }

            // PERF: dedupe vao bindings
            GL.BindVertexArray(vaoId);
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

        private void SetupGlobalUniform()
        {
            if (GlobalUniformHandle == default(int))
            {
                GL.GenBuffers(1, out GlobalUniformHandle);
                GL.BindBuffer(BufferTarget.UniformBuffer, GlobalUniformHandle);
                GL.BufferData(BufferTarget.UniformBuffer, GlobalUniform.Size, IntPtr.Zero, BufferUsageHint.DynamicDraw);
            }
            else
            {
                GL.BindBuffer(BufferTarget.UniformBuffer, GlobalUniformHandle);
            }

            GL.BufferSubData(BufferTarget.UniformBuffer, IntPtr.Zero, GlobalUniform.Size, ref GlobalUniform);

            GL.BindBufferBase(BufferRangeTarget.UniformBuffer, 0, GlobalUniformHandle);
            GL.BindBuffer(BufferTarget.UniformBuffer, 0);
        }

        private void SetupGenericUniform<T>(Shader shader, T uniform, int size) where T : struct
        {
            if (uniformHandles.TryGetValue(shader, out var handle) == false)
            {
                GL.GenBuffers(1, out handle);
                GL.BindBuffer(BufferTarget.UniformBuffer, handle);
                GL.BufferData(BufferTarget.UniformBuffer, size, IntPtr.Zero, BufferUsageHint.DynamicDraw);

                uniformHandles[shader] = handle;
            }
            else
            {
                GL.BindBuffer(BufferTarget.UniformBuffer, handle);
            }

            GL.BufferSubData(BufferTarget.UniformBuffer, IntPtr.Zero, size, ref uniform);

            GL.BindBufferBase(BufferRangeTarget.UniformBuffer, 1, handle);
            GL.BindBuffer(BufferTarget.UniformBuffer, 0);
        }

        private Dictionary<Shader, Action> shaderBeginActions = new Dictionary<Shader, Action>()
        {
            { Shader.Skybox, () => {
                GL.Disable(EnableCap.DepthTest);
            }},
            { Shader.Generic, () => {
                GL.Enable(EnableCap.DepthTest);
            }}
        };

        private Dictionary<Shader, Action> shaderEndActions = new Dictionary<Shader, Action>()
        {
            
        };
    }
}
