using OpenH2.Audio;
using OpenH2.Audio.Abstractions;
using Silk.NET.OpenAL;
using Silk.NET.OpenAL.Native.Extensions.EXT;
using System;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading;

namespace OpenH2.OpenAL.Audio
{
    public static class ALExt
    {
        public static SourceState GetSourceState(this AL al, uint source)
        {
            al.GetSourceProperty(source, GetSourceInteger.SourceState, out int val);
            return (SourceState)val;
        }
    }

    public class ALSoundEmitter : ISoundEmitter, IDisposable
    {
        private readonly uint source;
        private readonly AL al;
        private uint primarySamples;
        private int primarySampleRate;
        private uint primaryBuffer;
        private uint backBuffer;

        public ALSoundEmitter(AL al)
        {
            this.source = al.GenSource();
            this.primaryBuffer = al.GenBuffer();
            this.backBuffer = al.GenBuffer();
            this.al = al;
        }

        public void PlayImmediate<TSample>(AudioEncoding encoding, SampleRate rate, Span<TSample> data) where TSample : unmanaged
        {
            Interlocked.Exchange(ref backBuffer, Interlocked.Exchange(ref primaryBuffer, backBuffer));

            if(al.GetSourceState(this.source) != SourceState.Stopped)
                al.SourceStop(this.source);

            this.primarySamples = BufferData(encoding, rate, data, this.primaryBuffer);
            this.primarySampleRate = rate.Rate;
            al.SetSourceProperty(this.source, SourceInteger.Buffer, this.primaryBuffer);
            al.SourcePlay(this.source);
        }

        public void QueueSound()
        {
            throw new NotImplementedException();
        }

        public TimeSpan RemainingTime()
        {
            var state = al.GetSourceState(this.source);

            if(state != SourceState.Playing)
            {
                return TimeSpan.Zero;
            }

            al.GetSourceProperty(this.source, GetSourceInteger.SampleOffset, out var currentSample);

            var remainingSeconds = (this.primarySamples - currentSample) / (float)this.primarySampleRate;

            return TimeSpan.FromSeconds(remainingSeconds);
        }

        public void SetGain(float gain)
        {
            al.SetSourceProperty(this.source, SourceFloat.Gain, gain);
        }

        public void SetPosition(Vector3 position)
        {
            al.SetSourceProperty(this.source, SourceVector3.Position, position.X, position.Y, position.Z);
        }

        public void SetVelocity(Vector3 velocity)
        {
            al.SetSourceProperty(this.source, SourceVector3.Velocity, velocity.X, velocity.Y, velocity.Z);
        }

        public void Stop()
        {
            al.SourceStop(this.source);
        }

        private unsafe uint BufferData<TSample>(AudioEncoding encoding, SampleRate rate, Span<TSample> data, uint buffer) where TSample : unmanaged
        {
            var dataBytes = data.Length * Marshal.SizeOf<TSample>();

            var (format, samplesPerByte) = encoding switch
            {
                AudioEncoding.Mono16 => (BufferFormat.Mono16, 0.5f),
                AudioEncoding.MonoImaAdpcm => ((BufferFormat)IMA4BufferFormat.Mono, 2),
                AudioEncoding.StereoImaAdpcm => ((BufferFormat)IMA4BufferFormat.Stereo, 1),
            };

            fixed(TSample* pb = data)
                al.BufferData(buffer, format, pb, sizeof(TSample) * data.Length, rate.Rate);

            return (uint)(dataBytes * samplesPerByte);
        }

        public void Dispose()
        {
            if(this.source > 0)
            {
                Stop();
                al.DeleteBuffer(this.primaryBuffer);
                al.DeleteSource(this.source);
            }
        }
    }
}
