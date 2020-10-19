using CommandLine;
using OpenH2.Core.Extensions;
using OpenH2.Core.Factories;
using OpenH2.Core.Maps;
using OpenH2.Core.Maps.MCC;
using OpenH2.Core.Patching;
using OpenH2.Core.Scripting;
using OpenH2.Core.Scripting.LowLevel;
using OpenH2.Core.Tags.Scenario;
using OpenH2.Serialization;
using OpenH2.Serialization.Layout;
using System;
using System.IO;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;

namespace OpenH2.MccUtil
{
    [Verb("patch-tag")]
    public class PatchTagCommandLineArguments
    {
        [Option('p', "patch", Required = true, HelpText = "The tag patch to load")]
        public string TagPatchPath { get; set; }

        [Option('m', "map", HelpText = "The map to apply the patch to")]
        public string MapPath { get; set; }
    }


    public class PatchTagTask
    {
        private H2BaseMap scene;

        public PatchTagCommandLineArguments Args { get; }

        public static async Task Run(PatchTagCommandLineArguments args)
        {
            await new PatchTagTask(args).Run();
        }

        public PatchTagTask(PatchTagCommandLineArguments args)
        {
            this.Args = args;
        }

        public async Task Run()
        {
            // Load to determine where to write patches to
            // TODO: use mcc factory again
            //var factory = new MccMapFactory();
            //this.scene = factory.FromStream(map);

            var factory = new MapFactory(Path.GetDirectoryName(this.Args.MapPath), NullMaterialFactory.Instance);
            this.scene = factory.FromFile(new FileStream(this.Args.MapPath, FileMode.Open));

            using var inmemMap = new MemoryStream();
            using (var map = File.Open(this.Args.MapPath, FileMode.Open))
            {
                map.CopyTo(inmemMap);
                inmemMap.Position = 0;
            }

            var tagPatcher = new TagPatcher(scene, inmemMap);
            var patch = JsonSerializer.Deserialize<TagPatch>(File.ReadAllText(this.Args.TagPatchPath));
            tagPatcher.Apply(patch);

            inmemMap.Position = 0;
            var sig = H2vMap.CalculateSignature(inmemMap.ToArray());
            inmemMap.WriteInt32At(BlamSerializer.StartsAt<H2vMapHeader>(h => h.StoredSignature), sig);
            inmemMap.Position = 0;

            using (var map = File.Open(this.Args.MapPath, FileMode.Open))
            {
                inmemMap.CopyTo(map);
            }
        }
    }
}
