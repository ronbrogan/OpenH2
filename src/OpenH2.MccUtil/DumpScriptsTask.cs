using CommandLine;
using OpenH2.Core.Factories;
using OpenH2.Core.Scripting.Generation;
using OpenH2.Core.Tags.Scenario;
using OpenH2.ScriptAnalysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenH2.MccUtil
{
    [Verb("dump-scripts")]
    public class DumpScriptsCommandLineArguments
    {
        [Option('f', "file", Required = true, HelpText = "The map file to load")]
        public string File { get; set; }

        [Option('o', "output", HelpText = "Specify output directory of the script data")]
        public string OutputDirectory { get; set; } = $@"D:\h2scratch\scripts\mcc";
    }


    public class DumpScriptsTask
    {
        public DumpScriptsCommandLineArguments Args { get; }

        public static async Task Run(DumpScriptsCommandLineArguments args)
        {
            await new DumpScriptsTask(args).Run();
        }

        public DumpScriptsTask(DumpScriptsCommandLineArguments args)
        {
            this.Args = args;
        }

        public async Task Run()
        {
            var outRoot = this.Args.OutputDirectory;
            Directory.CreateDirectory(outRoot);

            var loader = new ScriptLoader(outRoot);

            var path = this.Args.File;

            if (File.Exists(path) == false)
            {
                path = Path.Combine(Environment.CurrentDirectory, path);
            }

            if (File.Exists(path) == false)
            {
                Console.WriteLine("Couldn't find file for " + this.Args.File);
                return;
            }

            var factory = new MccMapFactory();
            var scene = factory.FromFile(File.OpenRead(path));

            var scnr = scene.Scenario;

            var scenarioParts = scnr.Name.Split('\\', StringSplitOptions.RemoveEmptyEntries)
                    .Select(p => p.Trim())
                    .ToArray();

            var debugRoot = $@"{outRoot}\{scenarioParts.Last()}";
            Directory.CreateDirectory(debugRoot);

            foreach (var script in scnr.ScriptMethods)
            {
                var text = ScriptAnalysis.Program.GetScriptTree(scnr, script);
                var debugTree = ScriptTreeNode.ToString(text);
                File.WriteAllText(Path.Combine(debugRoot, script.Description + ".tree"), debugTree);
            }
        }
    }
}
