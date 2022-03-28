using System.Numerics;
using OpenH2.Audio.Abstractions;
using Silk.NET.OpenAL;

namespace OpenH2.OpenAL.Audio
{
    public class ALSoundListener : ISoundListener
    {
        private readonly AL al;
        private readonly Vector3 forward;
        private readonly Vector3 up;

        public ALSoundListener(AL al, Vector3 forward, Vector3 up)
        {
            al.SetListenerProperty(ListenerFloat.Gain, 1f);
            this.al = al;
            this.forward = forward;
            this.up = up;
        }

        public void SetPosition(Vector3 position)
        {
            al.SetListenerProperty(ListenerVector3.Position, position.X, position.Y, position.Z);
        }

        public unsafe void SetOrientation(Quaternion orientation)
        {
            var forward = Vector3.Transform(this.forward, orientation);

            float* vals = stackalloc[]
            {
                forward.X,
                forward.Y,
                forward.Z,
                up.X,
                up.Y,
                up.Z
            };

            al.SetListenerProperty(ListenerFloatArray.Orientation, vals);
        }
    }
}
