using OpenH2.Core.Architecture;
using OpenH2.Physics.Core;
using System.Collections.Generic;
using System.Linq;

namespace OpenH2.Engine.Components.Globals
{
    public class MaterialListComponent : Component
    {
        private Dictionary<int, PhysicsMaterial> physicsMaterials;

        public MaterialListComponent(Entity parent) : base(parent)
        {
            physicsMaterials = new Dictionary<int, PhysicsMaterial>();
        }

        public void AddPhysicsMaterial(PhysicsMaterial material)
        {
            physicsMaterials[material.Id] = material;
        }

        public PhysicsMaterial GetPhysicsMaterial(int id)
        {
            physicsMaterials.TryGetValue(id, out var mat);
            return mat;
        }

        public PhysicsMaterial[] GetPhysicsMaterials()
        {
            return physicsMaterials.Values.OrderBy(v => v.Id).ToArray();
        }
    }
}
