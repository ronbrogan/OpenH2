using OpenH2.Core.Factories;
using OpenH2.Core.Representations;
using OpenH2.Core.Tags;
using System;
using System.IO;

namespace OpenH2.TextureDumper
{
    class Program
    {
        static void Main(string[] args)
        {
            if(args.Length != 2)
            {
                Console.WriteLine("Must provide 2 arguments: map path, and output directory");
                return;
            }

            var mapPath = args[0];
            var outPath = args[1];

            if(File.Exists(mapPath) == false)
            {
                Console.WriteLine($"Error: Could not find {mapPath}");
                return;
            }

            Scene scene = null;

            using (var map = new FileStream(mapPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var factory = new SceneFactory();
                scene = factory.FromFile(map);
            }

            var processed = 0;

            foreach(var tag in scene.Tags)
            {
                var bitmTag = tag as BitmapTagNode;

                if (bitmTag == null)
                    continue;

                var writePath = Path.Combine(outPath, Path.GetDirectoryName(bitmTag.Meta.Name));
                var writeName = Path.GetFileName(bitmTag.Meta.Name) + ".dds";

                if(Directory.Exists(writePath) == false)
                {
                    Directory.CreateDirectory(writePath);
                }

                Console.WriteLine($"Writing {writeName} to {writePath}");

                File.WriteAllBytes(Path.Combine(writePath, writeName), bitmTag.Levels[0].ToArray());

                processed++;
            }

            Console.WriteLine($"Processed {processed} bitmaps from {Path.GetFileName(mapPath)}");
            Console.WriteLine("Press any key to exit");
            Console.ReadLine();
        }
    }
}
