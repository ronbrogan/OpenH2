namespace OpenH2.Physics.Core
{
    public class PhysicsMaterial
    {
        public int Id { get; }
        public float StaticFriction { get; set; }
        public float DynamicFriction { get; set; }
        public float Restitution { get; set; }

        public PhysicsMaterial(int id, float staticFriction, float dynamicFriction, float restitution)
        {
            this.Id = id;
            this.StaticFriction = staticFriction;
            this.DynamicFriction = dynamicFriction;
            this.Restitution = restitution;
        }
    }
}
