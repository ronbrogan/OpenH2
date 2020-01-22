using OpenH2.Core.Architecture;
using OpenH2.Core.Tags;
using OpenH2.Engine.Components;
using OpenH2.Engine.Stores;
using OpenH2.Rendering.Abstractions;
using OpenH2.Rendering.Pipelines;
using OpenH2.Rendering.Shaders;
using System.Linq;

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

            foreach (var (model, mx) in renderList.Models)
            {
                RenderingPipeline.AddStaticModel(model, mx);
            }

            foreach (var light in renderList.Lights)
            {
                RenderingPipeline.AddPointLight(light);
            }

            var cameras = world.Components<CameraComponent>();
            var cam = cameras.First();

            var pos = cam.PositionOffset;

            if (cam.TryGetSibling<TransformComponent>(out var xform))
            {
                pos += xform.Position;
            }

            var matrices = new GlobalUniform
            {
                ViewMatrix = cam.ViewMatrix,
                ProjectionMatrix = cam.ProjectionMatrix,
                ViewPosition = pos
            };

            graphics.BeginFrame(matrices);

            RenderingPipeline.DrawAndFlush();

            graphics.EndFrame();
        }
    }
}
