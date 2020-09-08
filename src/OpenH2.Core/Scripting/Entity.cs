namespace OpenH2.Core.Scripting
{
    public abstract class Entity 
    { 
        public EntityIdentifier Identifier { get; set; }

        public static implicit operator EntityIdentifier (Entity e) => e.Identifier;
    }
}