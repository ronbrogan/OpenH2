using OpenH2.Core.Architecture;
using OpenH2.Core.GameObjects;
using OpenH2.Engine.Components;
using System.Collections.Generic;

namespace OpenH2.Engine.Entities
{
    public class Vehicle : GameObjectEntity, IVehicle
    {
        public IGameObject Driver { get; private set; }

        public void SetComponents(
            TransformComponent xform,
            RigidBodyComponent body,
            params Component[] rest)
        {
            this.Transform = xform;
            this.Physics = body.PhysicsImplementation;

            var allComponents = new List<Component>();
            allComponents.Add(xform);
            allComponents.Add(body);
            allComponents.AddRange(rest);
            this.SetComponents(allComponents);
        }
    }
}
