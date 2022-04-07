namespace OpenH2.Foundation.Physics
{
    public interface IBody
    {
        ICollider Collider { get; }
        ITransform Transform { get; }        
    }
}
