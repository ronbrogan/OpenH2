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
using System.Diagnostics;
using System.Numerics;

namespace OpenH2.Rendering.OpenGL
{
    public class OpenGLGraphicsAdapter : IGraphicsAdapter
    {
        // Uniform index used in shaders
        private class UniformIndices
        {
            public const int Global = 0;
            public const int Shader = 1;
            public const int Transform = 2;
        }

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

        private int TransformUniformHandle;

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

        // PERF: sort calls by material and vao and deduplicate GL calls 
        public void DrawMesh(Mesh<BitmapTag> mesh)
        {
            SetupLighting();

            var bindings = SetupTextures(mesh.Material);

            CreateAndBindShaderUniform(mesh, bindings);

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

            mesh.Dirty = false;
        }

        private Dictionary<IMaterial<BitmapTag>, int[]> MaterialUniforms = new Dictionary<IMaterial<BitmapTag>, int[]>();
        private void CreateAndBindShaderUniform(Mesh<BitmapTag> mesh, MaterialBindings bindings)
        {
            if (MaterialUniforms.TryGetValue(mesh.Material, out var uniforms) == false)
            {
                uniforms = new int[(int)Shader.MAX_VALUE];
                MaterialUniforms[mesh.Material] = uniforms;
            }

            var existingUniformHandle = uniforms[(int)activeShader];

            // If the uniform was already buffered, we'll just reuse that buffered uniform
            // Currently these material uniforms never change at runtime - if this changes
            // there will have to be some sort of invalidation to ensure they're updated
            if(existingUniformHandle != 0)
            {
                GL.BindBufferBase(BufferRangeTarget.UniformBuffer, UniformIndices.Shader, existingUniformHandle);
                return;
            }

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
                        new GenericUniform(mesh.Material, bindings),
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
                case Shader.Pointviz:
                case Shader.TextureViewer:
                    break;
            }

            uniforms[(int)activeShader] = existingUniformHandle;
        }

        private long currentVao = -1;
        private void BindMesh(Mesh<BitmapTag> mesh)
        {
            if (mesh.Dirty || meshLookup.TryGetValue(mesh, out var vaoId) == false)
            {
                vaoId = UploadMesh(mesh);
            }

            if (currentVao != vaoId)
            {
                GL.BindVertexArray(vaoId);
                currentVao = vaoId;
            }
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

            GL.BindBufferBase(BufferRangeTarget.UniformBuffer, UniformIndices.Global, GlobalUniformHandle);
            GL.BindBuffer(BufferTarget.UniformBuffer, 0);
        }

        private void BindAndBufferShaderUniform<T>(Shader shader, T uniform, int size, out int handle) where T : struct
        {
            GL.GenBuffers(1, out handle);
            GL.BindBuffer(BufferTarget.UniformBuffer, handle);
            GL.BufferData(BufferTarget.UniformBuffer, size, ref uniform, BufferUsageHint.DynamicDraw);

            GL.BindBufferBase(BufferRangeTarget.UniformBuffer, UniformIndices.Shader, handle);
            GL.BindBuffer(BufferTarget.UniformBuffer, 0);
        }

        private void SetupTransformUniform(TransformUniform transform)
        {
            if (TransformUniformHandle == default(int))
            {
                GL.GenBuffers(1, out TransformUniformHandle);
                GL.BindBuffer(BufferTarget.UniformBuffer, TransformUniformHandle);
                GL.BufferData(BufferTarget.UniformBuffer, GlobalUniform.Size, IntPtr.Zero, BufferUsageHint.DynamicDraw);
            }
            else
            {
                GL.BindBuffer(BufferTarget.UniformBuffer, TransformUniformHandle);
            }

            GL.BufferSubData(BufferTarget.UniformBuffer, IntPtr.Zero, TransformUniform.Size, ref transform);

            GL.BindBufferBase(BufferRangeTarget.UniformBuffer, UniformIndices.Transform, TransformUniformHandle);
            GL.BindBuffer(BufferTarget.UniformBuffer, 0);
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

            GL.BindBufferBase(BufferRangeTarget.UniformBuffer, 3, LightingUniformHandle);
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
