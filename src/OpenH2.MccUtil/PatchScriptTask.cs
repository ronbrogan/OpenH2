using CommandLine;
using OpenH2.Core.Extensions;
using OpenH2.Core.Factories;
using OpenH2.Core.Maps.MCC;
using OpenH2.Core.Scripting;
using OpenH2.Core.Scripting.LowLevel;
using OpenH2.Core.Tags.Scenario;
using OpenBlam.Serialization;
using OpenBlam.Serialization.Layout;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace OpenH2.MccUtil
{
    [Verb("patch-script")]
    public class PatchScriptCommandLineArguments
    {
        [Option('s', "script", Required = true, HelpText = "The script patch to load")]
        public string ScriptPatchPath { get; set; }

        [Option('m', "map", HelpText = "The map to apply the patch to")]
        public string MapPath { get; set; }
    }


    public class PatchScriptTask
    {
        public PatchScriptCommandLineArguments Args { get; }

        public static async Task Run(PatchScriptCommandLineArguments args)
        {
            await new PatchScriptTask(args).Run();
        }

        public PatchScriptTask(PatchScriptCommandLineArguments args)
        {
            this.Args = args;
        }

        public async Task Run()
        {
            // TODO: check if map is decompressed already?
            // Load to determine where to write patches to
            using var map = File.Open(this.Args.MapPath, FileMode.Open);

            var factory = new UnifiedMapFactory(Path.GetDirectoryName(this.Args.MapPath));
            var h2map = factory.Load(Path.GetFileName(this.Args.MapPath));

            if(h2map is not H2mccMap mccMap)
            {
                throw new NotSupportedException("Only MCC maps are supported in this tool");
            }

            ScriptTreePatcher.PatchMap(mccMap, map, this.Args.ScriptPatchPath);
        }
    }
}
