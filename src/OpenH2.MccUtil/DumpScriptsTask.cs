using CommandLine;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using OpenH2.Core.Factories;
using OpenH2.Core.Maps.MCC;
using OpenH2.Core.Scripting.Generation;
using OpenH2.Core.Scripting.LowLevel;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OpenH2.MccUtil
{
    [Verb("dump-scripts", HelpText = "Dump the script trees and generate C# from the specified map files.")]
    public class DumpScriptsCommandLineArguments
    {
        [Option('d', "directory", Required = true, HelpText = "Working directory to search for files under")]
        public string WorkingDirectory { get; set; }

        [Option('f', "filter", HelpText = "File glob, defaults to *")]
        public string FileFilter { get; set; } = "*";

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
            var matcher = new Matcher();

            matcher.AddInclude(Args.FileFilter);

            var root = new DirectoryInfoWrapper(new DirectoryInfo(this.Args.WorkingDirectory ?? Environment.CurrentDirectory));

            Console.WriteLine($"Looking for files matching '{this.Args.FileFilter}' in '{root.FullName}'");

            var results = matcher.Execute(root);

            if (results.HasMatches == false)
            {
                Console.WriteLine("No matching files found");
                return;
            }

            if (this.Args.OutputDirectory != null)
            {
                Directory.CreateDirectory(this.Args.OutputDirectory);
            }

            var outRoot = this.Args.OutputDirectory;
            Directory.CreateDirectory(outRoot);

            foreach (var result in results.Files)
            {
                Console.WriteLine($"Found '{result.Path}', dumping...");

                DumpScriptsTo(Path.Combine(root.FullName, result.Path), outRoot);
            }
        }

        public void DumpScriptsTo(string path, string destination)
        {
            var loader = new ScriptLoader(destination);

            var rawMapStream = File.OpenRead(path);
            var mapStream = new MemoryStream();
            H2mccCompression.Decompress(rawMapStream, mapStream);
            mapStream.Position = 0;

            var factory = new MccMapFactory();
            var scene = factory.FromStream(mapStream);

            var scnr = scene.Scenario;
            loader.Load(scnr);

            var scenarioParts = scnr.Name.Split('\\', StringSplitOptions.RemoveEmptyEntries)
                    .Select(p => p.Trim())
                    .ToArray();

            var debugRoot = $@"{destination}\{scenarioParts.Last()}";
            Directory.CreateDirectory(debugRoot);

            for (int i = 0; i < scnr.ScriptMethods.Length; i++)
            {
                var script = scnr.ScriptMethods[i];
                var text = ScriptProcessor.GetScriptTree(scnr, script, i);
                var debugTree = text.ToString();
                File.WriteAllText(Path.Combine(debugRoot, script.Description + ".tree"), debugTree);
            }
        }
    }
}
