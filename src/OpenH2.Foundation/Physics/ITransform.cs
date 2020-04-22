using System.Numerics;

namespace OpenH2.Foundation.Physics
{
    public interface ITransform
    {
        Vector3 Scale { get; set; }
        Vector3 Position { get; set; }
        Quaternion Orientation { get; set; }
        Matrix4x4 TransformationMatrix { get; }

        void UpdateDerivedData();
        void UseTransformationMatrix(Matrix4x4 mat);
    }
}
