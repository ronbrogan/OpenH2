using OpenH2.Core.Architecture;
using OpenH2.Core.Representations;
using OpenH2.Core.Tags;
using OpenH2.Engine.Components;
using System.Numerics;

namespace OpenH2.Engine.Factories
{
    public static class RigidBodyFactory
    {
        public static RigidBodyComponent Create(Entity parent, TransformComponent transform, H2vMap map, TagRef<HaloModelTag> hlmtRef, int damageLevel = 0)
        {
            var inertiaTensor = Matrix4x4.Identity;
            var comOffset = Vector3.Zero;

            if (map.TryGetTag(hlmtRef, out var hlmt) &&
                map.TryGetTag(hlmt.PhysicsModel, out var phmo) &&
                phmo.BodyParameters.Length > 0)
            {
                inertiaTensor = phmo.BodyParameters[0].InertiaTensor;
                comOffset = phmo.BodyParameters[0].CenterOfMass;
            }

            var body = new RigidBodyComponent(parent, transform, inertiaTensor, comOffset)
            {
                Collider = ColliderFactory.GetColliderForHlmt(map, hlmtRef, damageLevel)
            };

            return body;
        }
    }
}
