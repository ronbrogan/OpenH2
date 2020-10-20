using CommandLine;
using OpenH2.Core.Extensions;
using OpenH2.Core.Factories;
using OpenH2.Core.Maps.MCC;
using OpenH2.Core.Patching;
using OpenH2.Core.Scripting.LowLevel;
using OpenH2.Serialization;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace OpenH2.MccBulkPatcher
{
    public class BulkPatchTaskArgs
    {
        [Option("patches", HelpText = "The directory to pull patches from, defaults to 'patches'")]
        public string PatchesDirectory { get; set; } = "patches";

        [Option("maps", HelpText = "Source of clean maps, will auto detect windows store MCC installation folder")]
        public string RawMapsDirectory { get; set; } = null;

        [Option("out", HelpText = "Destination of patched maps, will default to 'done' in current folder")]
        public string PatchedMapsDirectory { get; set; } = Path.Combine(Environment.CurrentDirectory, "done");
    }


    public class BulkPatchTask
    {
        public static void Run(BulkPatchTaskArgs args)
        {
            if(args.RawMapsDirectory == null)
            {
                // TODO: better
                args.RawMapsDirectory = @"C:\Program Files\ModifiableWindowsApps\HaloMCC\halo2\h2_maps_win64_dx11";
            }

            if(Directory.Exists(args.RawMapsDirectory) == false)
            {
                Console.WriteLine("Please enter the path to your 'clean' maps");
                Console.Write(">");

                args.RawMapsDirectory = Console.ReadLine().Trim();
            }

            var patches = FindDirectory(args.PatchesDirectory);
            var maps = FindDirectory(args.RawMapsDirectory);

            Directory.CreateDirectory(args.PatchedMapsDirectory);

            var patchDirs = Directory.GetDirectories(patches);

            Parallel.ForEach(patchDirs, patchDir =>
            {
                var mapName = Path.GetFileName(patchDir);
                var rawMapPath = Path.Combine(maps, mapName + ".map");
                var patchedMapPath = Path.Combine(args.PatchedMapsDirectory, mapName + ".map");

                if (File.Exists(rawMapPath) == false)
                {
                    throw new Exception($"Patch dir for '{mapName}', but no map found at '{rawMapPath}'");
                }

                var rawMap = File.OpenRead(rawMapPath);
                var patchedMap = new MemoryStream();

                H2mccCompression.Decompress(rawMap, patchedMap);

                var factory = new MccMapFactory();
                var scene = factory.FromStream(patchedMap);

                var patchFiles = Directory.GetFiles(patchDir, "*.tree", SearchOption.AllDirectories);

                foreach (var patchFile in patchFiles)
                {
                    Console.WriteLine($"Patching '{scene.Header.Name}' with '{patchFile.Substring(patchDir.Length)}'");
                    ScriptTreePatcher.PatchMap(scene, patchedMap, patchFile);
                }

                var tagPatches = Directory.GetFiles(patchDir, "*.tagpatch", SearchOption.AllDirectories);

                var tagPatcher = new TagPatcher(scene, patchedMap);

                foreach (var tagPatchPath in tagPatches)
                {
                    Console.WriteLine($"TagPatching '{scene.Header.Name}' with '{tagPatchPath.Substring(patchDir.Length)}'");

                    var settings = new JsonSerializerOptions() { ReadCommentHandling = JsonCommentHandling.Skip };
                    var patches = JsonSerializer.Deserialize<TagPatch[]>(File.ReadAllText(tagPatchPath), settings);
                    foreach (var patch in patches)
                        tagPatcher.Apply(patch);
                }

                var sig = H2mccMap.CalculateSignature(patchedMap);
                patchedMap.WriteUInt32At(BlamSerializer.StartsAt<H2mccMapHeader>(h => h.StoredSignature), (uint)sig);

                using var patchedFile = new FileStream(patchedMapPath, FileMode.Create);
                H2mccCompression.Compress(patchedMap, patchedFile);
            });

            Console.WriteLine("Done!");
        }

        private static string FindDirectory(string search)
        {
            var origSearch = search;

            if (Directory.Exists(search) == false)
            {
                search = Path.Combine(Environment.CurrentDirectory, search);
            }

            if (Directory.Exists(search) == false)
            {
                throw new Exception($"Unable to find directory '{origSearch}'");
            }

            return search;
        }
    }
}
