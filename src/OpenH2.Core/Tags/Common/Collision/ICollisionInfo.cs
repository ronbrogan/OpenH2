namespace OpenH2.Core.Tags.Common.Collision
{
    public interface ICollisionInfo
    {
        Node3D[] Node3Ds { get; }

        Plane[] Planes { get; }

        RawObject3[] RawObject3s { get; }

        RawObject4[] RawObject4s { get; }

        Node2D[] Node2Ds { get; }

        Face[] Faces { get; }

        HalfEdgeContainer[] HalfEdges { get; }

        Vertex[] Vertices { get; }
    }
}
