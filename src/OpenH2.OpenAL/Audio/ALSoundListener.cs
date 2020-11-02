using OpenH2.Audio.Abstractions;
using OpenTK.Audio.OpenAL;
using System.Numerics;

namespace OpenH2.OpenAL.Audio
{
    public class ALSoundListener : ISoundListener
    {
        private readonly Vector3 forward;
        private readonly Vector3 up;

        public ALSoundListener(Vector3 forward, Vector3 up)
        {
            AL.Listener(ALListenerf.Gain, 1f);
            this.forward = forward;
            this.up = up;
        }

        public void SetPosition(Vector3 position)
        {
            AL.Listener(ALListener3f.Position, position.X, position.Y, position.Z);
        }

        public void SetOrientation(Quaternion orientation)
        {
            var forward = Vector3.Transform(this.forward, orientation);
            var tkForward = new OpenTK.Mathematics.Vector3(forward.X, forward.Y, forward.Z);
            var tkUp = new OpenTK.Mathematics.Vector3(up.X, up.Y, up.Z);

            AL.Listener(ALListenerfv.Orientation, ref tkForward, ref tkUp);
        }
    }
}
