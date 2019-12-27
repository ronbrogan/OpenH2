using OpenH2.Core.Architecture;
using OpenH2.Core.Tags;
using OpenH2.Engine.Components;
using OpenH2.Foundation;
using System.Collections.Generic;
using System.Numerics;

namespace OpenH2.Engine.Stores
{
    /// <summary>
    /// RenderListStore is responsible for storing the objects that need to be passed to the RenderingPipeline
    /// Currently, this class is responsible for translating from engine models -> rendering models
    /// TBD if this translation is necessary
    /// </summary>
    public class RenderListStore
    {
        public List<(Model<BitmapTag>, Matrix4x4)> Models = new List<(Model<BitmapTag>, Matrix4x4)>();
        public List<PointLight> Lights = new List<PointLight>();

        public void Clear()
        {
            Models.Clear();
            Lights.Clear();
        }

        public void AddEntity(Entity entity)
        {
            Model<BitmapTag> model = default;
            var xformation = Matrix4x4.Identity;

            if(entity.TryGetChild<RenderModelComponent>(out var renderModel))
            {
                model = renderModel.RenderModel;

                xformation = model.CreateTransformationMatrix();
            }

            if (entity.TryGetChild<TransformComponent>(out var transform))
            {
                xformation = Matrix4x4.Multiply(xformation, transform.CreateTransformationMatrix());
            }

            Models.Add((model, xformation));

            if (entity.TryGetChild<PointLightEmitterComponent>(out var pointLight))
            {
                Lights.Add(new PointLight()
                {
                    Position = pointLight.Light.Position + xformation.Translation,
                    Color = pointLight.Light.Color,
                    Radius = pointLight.Light.Radius
                });
            }
        }
    }
}
