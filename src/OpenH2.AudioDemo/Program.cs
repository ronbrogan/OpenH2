using OpenH2.Audio;
using OpenH2.Core.Factories;
using OpenH2.Core.Maps.Vista;
using OpenH2.Core.Tags;
using OpenTK.Audio.OpenAL;
using OpenTK.Audio.OpenAL.Extensions.Creative.EnumerateAll;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace OpenH2.AudioDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting OpenAL!");
            var devices = ALC.GetStringList(GetEnumerationStringList.DeviceSpecifier);

            // Get the default device, then go though all devices and select the AL soft device if it exists.
            string deviceName = ALC.GetString(ALDevice.Null, AlcGetString.DefaultDeviceSpecifier);
            foreach (var d in devices)
            {
                if (d.Contains("OpenAL Soft"))
                {
                    deviceName = d;
                }
            }

            var device = ALC.OpenDevice(deviceName);
            var context = ALC.CreateContext(device, (int[])null);
            ALC.MakeContextCurrent(context);

            CheckALError("Start");

            // Playback the recorded data
            CheckALError("Before data");
            AL.GenBuffer(out int alBuffer);
            AL.GenBuffer(out int backBuffer);
            AL.Listener(ALListenerf.Gain, 0.1f);

            AL.GenSource(out int alSource);
            AL.Source(alSource, ALSourcef.Gain, 1f);

            // var get samples from map
            var map = @"D:\H2vMaps\01a_tutorial.map";
            var factory = new UnifiedMapFactory(Path.GetDirectoryName(map));
            var h2map = factory.Load(Path.GetFileName(map));

            if (h2map is not H2vMap scene)
            {
                throw new NotSupportedException("Only Vista maps are supported in this tool");
            }

            var soundMapping = scene.GetTag(scene.Globals.SoundInfos[0].SoundMap);

            var soundTags = scene.GetLocalTagsOfType<SoundTag>();

            var maxSoundId = soundTags.Max(s => s.SoundEntryIndex);

            // TODO: multiplayer sounds are referencing the wrong ugh! tag

            var i = 0;
            foreach (var snd in soundTags)
            {
                var enc = snd.Encoding switch
                {
                    EncodingType.ImaAdpcmMono => AudioEncoding.MonoImaAdpcm,
                    EncodingType.ImaAdpcmStereo => AudioEncoding.StereoImaAdpcm,
                    _ => AudioEncoding.Mono16,
                };

                var sr = snd.SampleRate switch
                {
                    Core.Tags.SampleRate.hz22k05 => Audio.SampleRate._22k05,
                    Core.Tags.SampleRate.hz44k1 => Audio.SampleRate._44k1,
                    _ => Audio.SampleRate._44k1
                };

                if (enc != AudioEncoding.Mono16)
                    continue;

                var name = snd.Name.Substring(snd.Name.LastIndexOf("\\", snd.Name.LastIndexOf("\\") - 1) + 1).Replace('\\', '_');

                Console.WriteLine($"[{i++}] {snd.Option1}-{snd.Option2}-{snd.Option3}-{snd.SampleRate}-{snd.Encoding}-{snd.Format2}-{snd.Unknown}-{snd.UsuallyMaxValue}-{snd.UsuallyZero} {name}");

                var filenameFormat = $"{name}.{snd.SampleRate}-{snd.Encoding}-{snd.Format2}-{snd.Unknown}-{snd.UsuallyZero}-{snd.UsuallyMaxValue}.{{0}}.sound";

                var soundEntry = soundMapping.SoundEntries[snd.SoundEntryIndex];

                for (var s = 0; s < soundEntry.NamedSoundClipCount; s++)
                {
                    var clipIndex = soundEntry.NamedSoundClipIndex + s;

                    var clipInfo = soundMapping.NamedSoundClips[clipIndex];

                    var clipFilename = string.Format(filenameFormat, s);

                    var clipSize = 0;
                    for (var c = 0; c < clipInfo.SoundDataChunkCount; c++)
                    {
                        var chunk = soundMapping.SoundDataChunks[clipInfo.SoundDataChunkIndex + c];
                        clipSize += (int)(chunk.Length & 0x3FFFFFFF);
                    }

                    Span<byte> clipData = new byte[clipSize];
                    var clipDataCurrent = 0;

                    for (var c = 0; c < clipInfo.SoundDataChunkCount; c++)
                    {
                        var chunk = soundMapping.SoundDataChunks[clipInfo.SoundDataChunkIndex + c];

                        var len = (int)(chunk.Length & 0x3FFFFFFF);
                        var chunkData = scene.ReadData(chunk.Offset.Location, chunk.Offset, len);

                        chunkData.Span.CopyTo(clipData.Slice(clipDataCurrent));
                        clipDataCurrent += len;
                    }

                    Interlocked.Exchange(ref backBuffer, Interlocked.Exchange(ref alBuffer, backBuffer));
                    AL.SourceStop(alSource);
                    BufferData(enc, sr, clipData.Slice(96), alBuffer);
                    CheckALError("After buffer");
                    AL.Source(alSource, ALSourcei.Buffer, alBuffer);
                    AL.SourcePlay(alSource);
                    
                    while(AL.GetSourceState(alSource) == ALSourceState.Playing)
                    {
                        Thread.Sleep(100);
                    }

                    // Only play first variant
                    break;
                }
            }

            ALC.MakeContextCurrent(ALContext.Null);
            ALC.DestroyContext(context);
            ALC.CloseDevice(device);

            Console.WriteLine("done");
            Console.ReadLine();
        }

        private static int BufferData<TSample>(AudioEncoding encoding, Audio.SampleRate rate, Span<TSample> data, int buffer) where TSample : unmanaged
        {
            var dataBytes = data.Length * Marshal.SizeOf<TSample>();

            var (format, samplesPerByte) = encoding switch
            {
                AudioEncoding.Mono16 => (ALFormat.Mp3Ext, 0.5f),
                AudioEncoding.MonoImaAdpcm => (ALFormat.MonoIma4Ext, 2),
                AudioEncoding.StereoImaAdpcm => (ALFormat.StereoIma4Ext, 1),
            };

            AL.BufferData(buffer, format, data, rate.Rate);
            return (int)(dataBytes * samplesPerByte);
        }

        // You can define other methods, fields, classes and namespaces here
        public static void CheckALError(string str)
        {
            ALError error = AL.GetError();
            if (error != ALError.NoError)
            {
                Console.WriteLine($"ALError at '{str}': {AL.GetErrorString(error)}");
            }
        }
    }
}
