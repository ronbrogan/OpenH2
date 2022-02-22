using OpenH2.Core.Architecture;
using OpenH2.Core.Tags;
using OpenH2.Engine.Components;
using OpenH2.Engine.Stores;
using OpenH2.Rendering.Abstractions;
using OpenH2.Rendering.Pipelines;
using OpenH2.Rendering.Shaders;
using System;
using System.Diagnostics;
using System.Linq;
using System.Numerics;

namespace OpenH2.Engine.Systems
{
    public class RenderPipelineSystem : RenderSystem
    {
        private readonly IGraphicsAdapter graphics;
        public IRenderingPipeline<BitmapTag> RenderingPipeline;

        public RenderPipelineSystem(World world, IGraphicsAdapter graphics) : base(world)
        {
            RenderingPipeline = new ForwardRenderingPipeline(graphics);
            this.graphics = graphics;
        }

        public override void Render(double timestep)
        {
            var renderList = world.GetGlobalResource<RenderListStore>();

            RenderingPipeline.SetModels(renderList.Models);

            foreach (var light in renderList.Lights)
            {
                RenderingPipeline.AddPointLight(light);
            }

            var cameras = world.Components<CameraComponent>();
            var cam = cameras.FirstOrDefault();

            if (cam == null)
                return;

            var pos = cam.PositionOffset;
            var orient = Quaternion.Identity;

            if (cam.TryGetSibling<TransformComponent>(out var xform))
            {
                pos += xform.Position;
                orient = Quaternion.Normalize(xform.Orientation);
            }

            var matrices = new GlobalUniform
            {
                ViewMatrix = cam.ViewMatrix,
                ProjectionMatrix = cam.ProjectionMatrix,
                SunLightMatrix0 = Matrix4x4.Identity,
                SunLightMatrix1 = Matrix4x4.Identity,
                SunLightMatrix2 = Matrix4x4.Identity,
                SunLightMatrix3 = Matrix4x4.Identity,
                SunLightDirection = new Vector3(0, 1, -1),
                ViewPosition = pos
            };

            const float ShadowMapFar = 117;
            var skylight = world.Components<SkyLightComponent>().FirstOrDefault();
            if (skylight != null)
            {
                matrices.SunLightDirection = skylight.Direction;

                var end0 = 5;
                var end1 = 15;
                var end2 = 50;

                matrices.SunLightDistances = new Vector4(end0, end1, end2, ShadowMapFar);

                matrices.SunLightMatrix0 = CreateFrustumLightMatrix(cam, 0.1f, end0, skylight.Direction);
                matrices.SunLightMatrix1 = CreateFrustumLightMatrix(cam, end0, end1, skylight.Direction);
                matrices.SunLightMatrix2 = CreateFrustumLightMatrix(cam, end1, end2, skylight.Direction);
                matrices.SunLightMatrix3 = CreateFrustumLightMatrix(cam, end2, ShadowMapFar, skylight.Direction);
            }

            RenderingPipeline.SetGlobals(matrices);

            graphics.BeginFrame(matrices);

            RenderingPipeline.DrawAndFlush();

            graphics.EndFrame();

            renderList.Clear();
        }

        Matrix4x4 CreateFrustumLightMatrix(CameraComponent cam, float near, float far, Vector3 lightDirection)
        {
            var frustum = Matrix4x4.CreatePerspectiveFieldOfView(cam.FieldOfView, cam.AspectRatio, near, far);
            var view = cam.ViewMatrix;

            Span<Vector4> corners = stackalloc Vector4[8];

            GetFrustumCornersWorldSpace(ref frustum, ref view, corners);

            var center = new Vector3(0, 0, 0);
            foreach (var v in corners)
            {
                center += new Vector3(v.X, v.Y, v.Z);
            }

            center /= corners.Length;

            var lightView = Matrix4x4.CreateLookAt(center + Vector3.Normalize(-lightDirection), center, new Vector3(1.0f, 0.0f, 0.0f));


            var minX = float.MaxValue;
            var maxX = float.MinValue;
            var minY = float.MaxValue;
            var maxY = float.MinValue;
            var minZ = float.MaxValue;
            var maxZ = float.MinValue;
            foreach (var v in corners)
            {
                var trf = Vector4.Transform(v, lightView);
                minX = MathF.Min(minX, trf.X);
                maxX = MathF.Max(maxX, trf.X);
                minY = MathF.Min(minY, trf.Y);
                maxY = MathF.Max(maxY, trf.Y);
                minZ = MathF.Min(minZ, trf.Z);
                maxZ = MathF.Max(maxZ, trf.Z);
            }

            float zMult = 10.0f;
            if (minZ < 0)
            {
                minZ *= zMult;
            }
            else
            {
                minZ /= zMult;
            }
            if (maxZ < 0)
            {
                maxZ /= zMult;
            }
            else
            {
                maxZ *= zMult;
            }

            var lightProjection = Matrix4x4.CreateOrthographicOffCenter(minX, maxX, minY, maxY, minZ, maxZ);

            return Matrix4x4.Multiply(lightView, lightProjection);
        }

        void GetFrustumCornersWorldSpace(ref Matrix4x4 proj, ref Matrix4x4 view, Span<Vector4> frustumCorners)
        {
            Matrix4x4.Invert(Matrix4x4.Multiply(view, proj), out var inv);

            var i = 0;
            for (var x = 0; x < 2; ++x)
            {
                for (var y = 0; y < 2; ++y)
                {
                    for (var z = 0; z < 2; ++z)
                    {
                        var corner = new Vector4(
                            2.0f * x - 1.0f,
                            2.0f * y - 1.0f,
                            2.0f * z - 1.0f,
                            1.0f);

                        var pt =  Vector4.Transform(corner, inv);

                        frustumCorners[i++] = pt / pt.W;
                    }
                }
            }

            Debug.Assert(i == 8);
        }

        private void SetupShadowCascades(ref GlobalUniform matrices)
        { 
        }
    }
}
