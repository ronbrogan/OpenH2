using OpenH2.Core.Factories;
using OpenH2.Core.Representations;
using OpenH2.Translation;
using System;
using System.IO;
using OpenH2.Translation.TagData;
using System.Linq;
using OpenH2.Core.Tags;

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

            H2vMap scene = null;

            using (var map = new FileStream(mapPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var factory = new MapFactory();
                scene = factory.FromFile(map);
            }


            var translator = new TagTranslator(scene);

            var bitmaps = scene.Tags.Where(t => t.Value is BitmapTag).Select(t => t.Value as BitmapTag);

            var processed = 0;

            foreach(var bitmap in bitmaps)
            {
                var writePath = Path.Combine(outPath, Path.GetDirectoryName(bitmap.Name));
                var writeName = Path.GetFileName(bitmap.Name) + ".dds";

                if(Directory.Exists(writePath) == false)
                {
                    Directory.CreateDirectory(writePath);
                }

                Console.WriteLine($"Writing {writeName} to {writePath}");

                // Decompress and synthesize texture headers
                for (var i = 0; i < bitmap.LevelsOfDetail.Length; i++)
                {
                    var lod = bitmap.LevelsOfDetail[i];

                    if (lod.Data.IsEmpty)
                        continue;

                    var ms = File.OpenWrite(Path.Combine(writePath, writeName));
                    WriteTextureHeader(bitmap, ms);
                    ms.Write(lod.Data.ToArray(), 0, lod.Data.Length);
                }

                processed++;
            }

            Console.WriteLine($"Processed {processed} bitmaps from {Path.GetFileName(mapPath)}");
            Console.WriteLine("Press any key to exit");
            Console.ReadLine();
        }

        public static void WriteTextureHeader(BitmapTag bitm, Stream destination)
        {
            var ddsHeader = new DdsHeader(
                bitm.TextureFormat,
                bitm.TextureType,
                bitm.Width,
                bitm.Height,
                bitm.Depth,
                bitm.MipMapCount,
                null,
                null);

            ddsHeader.HeaderData.CopyTo(destination);
        }
    }
}
