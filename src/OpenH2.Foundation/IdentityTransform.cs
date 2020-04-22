using OpenH2.Foundation.Physics;
using System.Numerics;

namespace OpenH2.Foundation
{
    /// <summary>
    /// An immutable transform that represents no transformation
    /// </summary>
    public class IdentityTransform : ITransform
    {
        private static IdentityTransform instance;

        public static IdentityTransform Instance()
        {
            if(instance == null)
            {
                instance = new IdentityTransform();
            }

            return instance;
        }

        private IdentityTransform() { }

        public Vector3 Scale { get => Vector3.One; set { } }
        public Vector3 Position { get => Vector3.Zero; set { } }
        public Quaternion Orientation { get => Quaternion.Identity; set { } }

        public Matrix4x4 TransformationMatrix => Matrix4x4.Identity;

        public void UpdateDerivedData()
        {
        }

        public void UseTransformationMatrix(Matrix4x4 mat)
        {
        }
    }
}
