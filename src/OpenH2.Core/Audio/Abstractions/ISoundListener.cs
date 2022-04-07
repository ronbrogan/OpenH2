using System.Numerics;

namespace OpenH2.Core.Audio.Abstractions
{
    public interface ISoundListener
    {
        void SetPosition(Vector3 position);
        void SetOrientation(Quaternion orientation);
    }
}
