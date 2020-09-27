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
            Directory.CreateDirectory(outRoot);

            Debug.Assert(args.Length > 0 
                && File.Exists(args[0]));

            var map = args[0];

            var factory = new MapFactory(Path.GetDirectoryName(map), NullMaterialFactory.Instance);
            var scene = factory.FromFile(File.OpenRead(map));

            var soundMapping = scene.GetTag(scene.Globals.SoundInfos[0].SoundMap);

            var soundTags = scene.GetLocalTagsOfType<SoundTag>();

            var maxSoundId = soundTags.Max(s => s.SoundIndex);

            var i = 0;
            foreach (var snd in soundTags)
            {
                var name = snd.Name.Substring(snd.Name.LastIndexOf("\\", snd.Name.LastIndexOf("\\") - 1));

                Console.WriteLine($"[{i++}] {snd.Option1}-{snd.Option2}-{snd.Option3}-{snd.Option4}-{snd.Flags}-{snd.Unknown}-{snd.UsuallyMaxValue}-{snd.UsuallyZero} {name}");
            }
        }
    }
}
