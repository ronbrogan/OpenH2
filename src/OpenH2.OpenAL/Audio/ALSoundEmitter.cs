using OpenH2.Audio;
using OpenH2.Audio.Abstractions;
using OpenTK.Audio.OpenAL;
using System;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading;

namespace OpenH2.OpenAL.Audio
{
    public class ALSoundEmitter : ISoundEmitter, IDisposable
    {
        private readonly int source;
        private int primarySamples;
        private int primarySampleRate;
        private int primaryBuffer;
        private int backBuffer;

        public ALSoundEmitter()
        {
            this.source = AL.GenSource();
            this.primaryBuffer = AL.GenBuffer();
            this.backBuffer = AL.GenBuffer();
        }

        public void PlayImmediate<TSample>(AudioEncoding encoding, SampleRate rate, Span<TSample> data) where TSample : unmanaged
        {
            Interlocked.Exchange(ref backBuffer, Interlocked.Exchange(ref primaryBuffer, backBuffer));

            if(AL.GetSourceState(this.source) != ALSourceState.Stopped)
                AL.SourceStop(this.source);

            this.primarySamples = BufferData(encoding, rate, data, this.primaryBuffer);
            this.primarySampleRate = rate.Rate;
            AL.Source(this.source, ALSourcei.Buffer, this.primaryBuffer);
            AL.SourcePlay(this.source);
        }

        public void QueueSound()
        {
            throw new NotImplementedException();
        }

        public TimeSpan RemainingTime()
        {
            var state = AL.GetSourceState(this.source);

            if(state != ALSourceState.Playing)
            {
                return TimeSpan.Zero;
            }

            AL.GetSource(this.source, ALGetSourcei.SampleOffset, out var currentSample);

            var remainingSeconds = (this.primarySamples - currentSample) / (float)this.primarySampleRate;

            return TimeSpan.FromSeconds(remainingSeconds);
        }

        public void SetGain(float gain)
        {
            AL.Source(this.source, ALSourcef.Gain, gain);
        }

        public void SetPosition(Vector3 position)
        {
            AL.Source(this.source, ALSource3f.Position, position.X, position.Y, position.Z);
        }

        public void SetVelocity(Vector3 velocity)
        {
            AL.Source(this.source, ALSource3f.Velocity, velocity.X, velocity.Y, velocity.Z);
        }

        public void Stop()
        {
            AL.SourceStop(this.source);
        }

        private int BufferData<TSample>(AudioEncoding encoding, SampleRate rate, Span<TSample> data, int buffer) where TSample : unmanaged
        {
            var dataBytes = data.Length * Marshal.SizeOf<TSample>();

            var (format, samplesPerByte) = encoding switch
            {
                AudioEncoding.Mono16 => (ALFormat.Mono16, 0.5f),
                AudioEncoding.MonoImaAdpcm => (ALFormat.MonoIma4Ext, 2),
                AudioEncoding.StereoImaAdpcm => (ALFormat.StereoIma4Ext, 1),
            };

            AL.BufferData(buffer, format, data, rate.Rate);
            return (int)(dataBytes * samplesPerByte);
        }

        public void Dispose()
        {
            if(this.source > 0)
            {
                Stop();
                AL.DeleteBuffer(this.primaryBuffer);
                AL.DeleteSource(this.source);
            }
        }
    }
}
