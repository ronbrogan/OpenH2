using OpenH2.Audio;
using OpenH2.Core.Factories;
using OpenH2.Core.Tags;
using OpenH2.OpenAL.Audio;
using OpenTK.Audio.OpenAL;
using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;

namespace OpenH2.AudioDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            var host = OpenALHost.Open(new Vector3(1, 0, 0), new Vector3(0, 0, 1));
            var adapter = host.GetAudioAdapter();
            var listener = adapter.CreateListener();
            var emitter = adapter.CreateEmitter();

            //Span<short> sine1 = new short[44100 * 1];
            //FillSine(sine1, 640, 44100);
            //emitter.SetGain(0.1f);
            //emitter.PlayImmediate(AudioEncoding.Mono16, SampleRate._44k1, sine1);

            var map = @"D:\H2vMaps\01a_tutorial.map";

            var factory = new MapFactory(Path.GetDirectoryName(map), NullMaterialFactory.Instance);
            var scene = factory.FromFile(File.OpenRead(map));

            var soundMapping = scene.GetTag(scene.Globals.SoundInfos[0].SoundMap);

            var soundTags = scene.GetLocalTagsOfType<SoundTag>().Skip(22);

            var maxSoundId = soundTags.Max(s => s.SoundEntryIndex);

            // TODO: multiplayer sounds are referencing the wrong ugh! tag

            var i = 0;
            foreach (var snd in soundTags)
            {
                var name = snd.Name.Substring(snd.Name.LastIndexOf("\\", snd.Name.LastIndexOf("\\") - 1) + 1).Replace('\\', '_');

                Console.WriteLine($"[{i++}] {snd.Option1}-{snd.Option2}-{snd.Option3}-{snd.SampleRate}-{snd.Encoding}-{snd.Format2}-{snd.Unknown}-{snd.UsuallyMaxValue}-{snd.UsuallyZero} {name}");

                var filenameFormat = $"{name}.{snd.SampleRate}-{snd.Encoding}-{snd.Format2}-{snd.Unknown}-{snd.UsuallyZero}-{snd.UsuallyMaxValue}.{{0}}.sound";

                var soundEntry = soundMapping.SoundEntries[snd.SoundEntryIndex];

                for (var s = 0; s < soundEntry.NamedSoundClipCount; s++)
                {
                    var clipIndex = soundEntry.NamedSoundClipIndex + s;

                    var clipInfo = soundMapping.NamedSoundClips[clipIndex];

                    var clipFilename = string.Format(filenameFormat, s);

                    using var clipData = new MemoryStream();

                    for (var c = 0; c < clipInfo.SoundDataChunkCount; c++)
                    {
                        var chunk = soundMapping.SoundDataChunks[clipInfo.SoundDataChunkIndex + c];

                        var chunkData = scene.ReadData(chunk.Offset.Location, chunk.Offset, (int)(chunk.Length & 0x3FFFFFFF));

                        clipData.Write(chunkData.Span);
                    }

                    clipData.Position = 0;

                    emitter.PlayImmediate<byte>(AudioEncoding.MonoImaAdpcm, Audio.SampleRate._44k1, clipData.ToArray());
                    Thread.Sleep(1000);
                    break;
                }
            }

            
            CheckALError("After playImmediate");

            adapter.DestroyEmitter(emitter);
            adapter.DestroyListener(listener);
            host.Shutdown();
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

        public static void FillSine(Span<short> buffer, float frequency, float sampleRate)
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = (short)(MathF.Sin((i * frequency * MathF.PI * 2) / sampleRate) * short.MaxValue);
            }
        }
    }
}
