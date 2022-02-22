using OpenH2.Core.Architecture;
using OpenH2.Core.Tags;
using OpenH2.Engine.Components;
using OpenH2.Engine.Stores;
using OpenH2.Rendering.Abstractions;
using OpenH2.Rendering.Pipelines;
using OpenH2.Rendering.Shaders;
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
                SunLightMatrix = Matrix4x4.Identity,
                SunLightDirection = new Vector3(0, 1, -1),
                ViewPosition = pos
            };

            var skylight = world.Components<SkyLightComponent>().FirstOrDefault();
            if (skylight != null)
            {
                matrices.SunLightDirection = skylight.Direction;

                var forward = Vector3.Transform(EngineGlobals.Forward, orient);

                var offset = (forward * 20);

                // 20 units in front of camera
                var sunTarget = pos + offset;

                // above target in direction of light
                var sunPos = sunTarget - skylight.Direction;

                var look = Matrix4x4.CreateLookAt(sunPos, sunTarget, forward);
                var proj = Matrix4x4.CreateOrthographic(40, 40, 0.01f, 100);
                
                matrices.SunLightMatrix = Matrix4x4.Multiply(look, proj);
            }

            RenderingPipeline.SetGlobals(matrices);

            graphics.BeginFrame(matrices);

            RenderingPipeline.DrawAndFlush();

            graphics.EndFrame();

            renderList.Clear();
        }
    }
}
