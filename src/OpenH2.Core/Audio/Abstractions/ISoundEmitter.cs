using System;
using System.Numerics;

namespace OpenH2.Core.Audio.Abstractions
{
    public interface ISoundEmitter
    {
        void SetPosition(Vector3 position);
        void SetGain(float gain);
        void QueueSound();
        void PlayImmediate<TSample>(AudioEncoding encoding, SampleRate rate, Span<TSample> data) where TSample : unmanaged;
        void Stop();
        TimeSpan RemainingTime();
    }
}
