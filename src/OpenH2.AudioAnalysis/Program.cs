using OpenH2.Core.Factories;
using OpenH2.Core.Tags;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace OpenH2.AudioAnalysis
{
    class Program
    {
        static void Main(string[] args)
        {
            var outRoot = $@"D:\h2scratch\sounds";

            Debug.Assert(args.Length > 0 
                && File.Exists(args[0]));

            var map = args[0];

            var factory = new MapFactory(Path.GetDirectoryName(map), NullMaterialFactory.Instance);
            var scene = factory.FromFile(File.OpenRead(map));

            var scenarioOut = Path.Combine(outRoot, scene.Header.Name);
            Directory.CreateDirectory(scenarioOut);

            var soundMapping = scene.GetTag(scene.Globals.SoundInfos[0].SoundMap);

            var soundTags = scene.GetLocalTagsOfType<SoundTag>();

            var maxSoundId = soundTags.Max(s => s.SoundEntryIndex);



            var i = 0;
            foreach (var snd in soundTags)
            {
                var name = snd.Name.Substring(snd.Name.LastIndexOf("\\", snd.Name.LastIndexOf("\\") - 1) + 1).Replace('\\', '_');

                Console.WriteLine($"[{i++}] {snd.Option1}-{snd.Option2}-{snd.Option3}-{snd.Option4}-{snd.Flags}-{snd.Unknown}-{snd.UsuallyMaxValue}-{snd.UsuallyZero} {name}");

                var filenameFormat = $"{name}.{snd.Option1}-{snd.Option2}-{snd.Option3}-{snd.Option4}.{{0}}.sound";

                var soundEntry = soundMapping.SoundEntries[snd.SoundEntryIndex];

                for(var s = 0; s < soundEntry.NamedSoundClipCount; s++)
                {
                    var clipIndex = soundEntry.NamedSoundClipIndex + s;

                    var clipInfo = soundMapping.NamedSoundClips[clipIndex];

                    var clipFilename = string.Format(filenameFormat, s);

                    //using var clipData = new FileStream(Path.Combine(scenarioOut, clipFilename), FileMode.Create);
                    //
                    //for(var c = 0; c < clipInfo.SoundDataChunkCount; c++)
                    //{
                    //    var chunk = soundMapping.SoundDataChunks[clipInfo.SoundDataChunkIndex + c];
                    //
                    //    var chunkData = scene.ReadData(chunk.Offset.Location, chunk.Offset, (int)(chunk.Length & 0x3FFFFFFF));
                    //
                    //    clipData.Write(chunkData.Span);
                    //}
                }
            }
        }
    }
}
