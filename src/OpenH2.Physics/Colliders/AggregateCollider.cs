using OpenH2.Foundation.Physics;
using System.Collections.Generic;

namespace OpenH2.Physics.Colliders
{
    public class AggregateCollider : ICollider
    {
        private List<ICollider> colliders = new ();

        public int PhysicsMaterial => 0;

        public IReadOnlyList<ICollider> ColliderComponents => colliders;

        public void AddCollider(ICollider collider)
        {
            this.colliders.Add(collider);
        }
    }
}
