using OpenBlam.Core.ExternalFormats;
using OpenH2.Core.Enums.Texture;
using OpenH2.Core.Factories;
using OpenH2.Core.Maps;
using OpenH2.Core.Tags;
using System;
using System.Collections.Generic;
using System.IO;
using Caps = OpenBlam.Core.ExternalFormats.DdsHeader.Caps;
using Caps2 = OpenBlam.Core.ExternalFormats.DdsHeader.Caps2;

namespace OpenH2.TextureDumper
{
    class Program
    {
        private static Dictionary<TextureType, Caps> CapsLookup = new Dictionary<TextureType, Caps>
        {
            { TextureType.Cubemap, Caps.Texture | Caps.Complex },
            { TextureType.Sprite, Caps.Texture },
            { TextureType.ThreeDimensional, Caps.Texture | Caps.Complex },
            { TextureType.TwoDimensional, Caps.Texture },
            { TextureType.UI, Caps.Texture },
        };

        private static Dictionary<TextureType, Caps2> Caps2Lookup = new Dictionary<TextureType, Caps2>
        {
            { TextureType.Cubemap, Caps2.Cubemap },
            { TextureType.Sprite, (Caps2)0 },
            { TextureType.ThreeDimensional, Caps2.Volume },
            { TextureType.TwoDimensional, (Caps2)0 },
            { TextureType.UI, (Caps2)0 },
        };

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
                var factory = new MapFactory(Path.GetDirectoryName(mapPath), new MaterialFactory(Environment.CurrentDirectory));
                scene = factory.FromFile(map);
            }

            var bitmaps = scene.GetLocalTagsOfType<BitmapTag>();

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
                for (var i = 0; i < bitmap.TextureInfos[0].LevelsOfDetail.Length; i++)
                {
                    var lod = bitmap.TextureInfos[0].LevelsOfDetail[i];

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
            var ddsHeader = DdsHeader.Create(
                bitm.TextureInfos[0].Format,
                CapsLookup[bitm.TextureType],
                Caps2Lookup[bitm.TextureType],
                bitm.TextureInfos[0].Width,
                bitm.TextureInfos[0].Height,
                bitm.TextureInfos[0].Depth,
                bitm.MipMapCount,
                null,
                null);

            destination.Write(ddsHeader);
        }
    }
}
