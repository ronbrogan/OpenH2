using OpenH2.Core.Architecture;
using OpenH2.Engine.Components;
using OpenH2.Engine.Stores;
using OpenH2.Foundation;
using OpenH2.Rendering;
using OpenH2.Rendering.Abstractions;
using System.Numerics;

namespace OpenH2.Engine.Systems
{
    public class RenderCollectorSystem : WorldSystem
    {
        private readonly IGraphicsAdapter graphics;

        public RenderCollectorSystem(World world, IGraphicsAdapter graphics) : base(world)
        {
            this.graphics = graphics;
        }

        public override void Update(double timestep)
        {
            var renderList = this.world.GetGlobalResource<RenderListStore>();

            var entities = this.world.Scene.Entities.Values;
            foreach(var entity in entities)
            {
                
                var rootTransform = Matrix4x4.Identity;

                if (entity.TryGetChild<TransformComponent>(out var transform))
                {
                    rootTransform = transform.TransformationMatrix;
                }

                if (entity.TryGetChild<RenderModelComponent>(out var renderModel))
                {
                    var localTransform = renderModel.RenderModel.CreateTransformationMatrix();
                    var finalTransform = Matrix4x4.Multiply(localTransform, rootTransform);

                    if (renderModel.DrawCommands == null)
                    {
                        graphics.UploadModel(renderModel.RenderModel, out var commands);
                        renderModel.UpdateDrawCommands(commands);
                    }

                    // BUG: If a model should be rendered by multiple shaders, (ie diffuse + wireframe) we'll need to 
                    // submit a draw group with unique draw command instances per shader. This is because the 
                    // shader uniform handle is stored on the draw commands. Alternatively, we could perhaps
                    // move the shader uniform handle to the draw group and submit same commands with different 
                    // wrapping DrawGroup instances
                    // If we get rid of rendering a single item in multiple shaders, this wouldn't be a problem

                    DrawGroup group = new DrawGroup()
                    {
                        DrawCommands = renderModel.DrawCommands,
                        Transform = finalTransform,
                        Flags = renderModel.RenderModel.Flags
                    };

                    renderList.Add(group);
                }

                if (entity.TryGetChild<PointLightEmitterComponent>(out var pointLight))
                {
                    renderList.Add(new PointLight()
                    {
                        Position = pointLight.Light.Position + rootTransform.Translation,
                        Color = pointLight.Light.Color,
                        Radius = pointLight.Light.Radius
                    });
                }
            }
        }
    }
}
