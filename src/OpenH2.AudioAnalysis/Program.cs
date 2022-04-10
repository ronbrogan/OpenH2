using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using OpenBlam.Core.ExternalFormats;
using OpenH2.Core.Factories;
using OpenH2.Core.Maps.Vista;
using OpenH2.Core.Tags;

namespace OpenH2.AudioAnalysis
{
    class Program
    {
        static void Main(string[] args)
        {
            var outRoot = $@"D:\h2scratch\sounds";

            Debug.Assert(args.Length > 0 
                && Directory.Exists(args[0]));

            var mapRoot = args[0];

            var factory = new MapFactory(mapRoot);

            var maps = Directory.EnumerateFiles(mapRoot, "*.map");
            foreach(var map in maps)
            {
                var h2map = factory.Load(Path.GetFileName(map));

                if (h2map is not H2vMap scene)
                {
                    throw new NotSupportedException("Only Vista maps are supported in this tool");
                }

                var scenarioOut = Path.Combine(outRoot, scene.Header.Name);
                Directory.CreateDirectory(scenarioOut);

                var soundMapping = scene.GetTag(scene.Globals.SoundInfos[0].SoundMap);

                var soundTags = scene.GetLocalTagsOfType<SoundTag>();

                var maxSoundId = soundTags.Max(s => s.SoundEntryIndex);

                // TODO: multiplayer sounds are referencing the wrong ugh! tag

                var i = 0;
                foreach (var snd in soundTags)
                {
                    var name = snd.Name.Substring(snd.Name.LastIndexOf("\\", snd.Name.LastIndexOf("\\") - 1) + 1).Replace('\\', '_');

                    if (snd.Name != "sound\\dialog\\combat\\sgt_johnson\\08_judge\\scld_plr")
                    {
                        continue;
                    }

                    Console.WriteLine($"[{i++}] {snd.SampleRate}-{snd.Encoding}-{snd.Format2}-{snd.Unknown}-{snd.UsuallyMaxValue}-{snd.UsuallyZero} {name}");

                    var filenameFormat = $"{name}.{snd.SampleRate}-{snd.Encoding}-{snd.Format2}-{snd.Unknown}-{snd.UsuallyZero}-{snd.UsuallyMaxValue}.{{0}}.sound";

                    var soundEntry = soundMapping.SoundEntries[snd.SoundEntryIndex];

                    for (var s = 0; s < soundEntry.NamedSoundClipCount; s++)
                    {
                        var clipIndex = soundEntry.NamedSoundClipIndex + s;

                        var clipInfo = soundMapping.NamedSoundClips[clipIndex];

                        var clipFilename = string.Format(filenameFormat, s);

                        var rawData = new MemoryStream();
                        using var clipData = new FileStream(Path.Combine(scenarioOut, clipFilename), FileMode.Create);

                        for (var c = 0; c < clipInfo.SoundDataChunkCount; c++)
                        {
                            var chunk = soundMapping.SoundDataChunks[clipInfo.SoundDataChunkIndex + c];

                            var chunkData = scene.ReadData(chunk.Offset.Location, chunk.Offset, (int)(chunk.Length & 0x3FFFFFFF));

                            rawData.Write(chunkData.Span);
                        }

                        var stereo = snd.Encoding == EncodingType.ImaAdpcmStereo;
                        var pcm = ImaAdpcmAudio.Decode(stereo, rawData.ToArray());
                        var bytes = MemoryMarshal.Cast<short, byte>(pcm.AsSpan()).ToArray();
                        clipData.Write(bytes);
                    }
                }
            }
        }
    }
}
