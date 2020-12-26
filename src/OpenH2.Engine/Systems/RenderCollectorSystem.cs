using OpenH2.Core.Architecture;
using OpenH2.Engine.Components;
using OpenH2.Engine.Stores;
using OpenH2.Foundation;
using OpenH2.Rendering;
using OpenH2.Rendering.Abstractions;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Diagnostics;
using System.Numerics;

namespace OpenH2.Engine.Systems
{
    public class RenderCollectorSystem : WorldSystem
    {
        private readonly IGraphicsAdapter graphics;
        private RenderListStore renderList;
        private InputStore inputStore;
        private RenderLayers enabledLayers = RenderLayers.Normal;

        public RenderCollectorSystem(World world, IGraphicsAdapter graphics) : base(world)
        {
            this.graphics = graphics;
        }

        public override void Initialize(Scene scene)
        {
            this.renderList = this.world.GetGlobalResource<RenderListStore>();
            this.inputStore = this.world.GetGlobalResource<InputStore>();
        }

        public override void Update(double timestep)
        {
            if (inputStore.WasPressed(Keys.F2))
            {
                enabledLayers ^= RenderLayers.Normal;
            }

            if (inputStore.WasPressed(Keys.F3))
            {
                enabledLayers ^= RenderLayers.Debug;
            }

            if (inputStore.WasPressed(Keys.F4))
            {
                enabledLayers ^= RenderLayers.Collision;
            }

            if (inputStore.WasPressed(Keys.F5))
            {
                enabledLayers ^= RenderLayers.Scripting;
            }

            var entities = this.world.Scene.Entities.Values;
            foreach(var entity in entities)
            {
                var rootTransform = Matrix4x4.Identity;

                if (entity.TryGetChild<TransformComponent>(out var transform))
                {
                    rootTransform = transform.TransformationMatrix;
                }

                var renderModels = entity.GetChildren<RenderModelComponent>();

                for (int j = 0; j < renderModels.Length; j++)
                {
                    var renderModel = renderModels[j];

                    if((renderModel.RenderModel.RenderLayer & this.enabledLayers) == 0)
                    {
                        continue;
                    }

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
