using CommandLine;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using OpenH2.Core.Factories;
using OpenH2.Core.Maps.MCC;
using OpenH2.Core.Scripting.Generation;
using OpenH2.Core.Scripting.LowLevel;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
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
        public string OutputDirectory { get; set; } = $@"D:\h2scratch\mcc\scripts";

        [Option('v', "verbose-trees", HelpText = "Create verbose script tree text")]
        public bool CreateVerboseTrees { get; set; }
    }


    public class DumpScriptsTask
    {
        public DumpScriptsCommandLineArguments Args { get; }
        private ConcurrentDictionary<string, HashSet<ushort>> MethodInfos = new ConcurrentDictionary<string, HashSet<ushort>>();

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

            File.WriteAllText(Path.Combine(outRoot, "MethodInfo.txt"), GenerateBuiltinInfo());
        }

        public void DumpScriptsTo(string path, string destination)
        {
            var loader = new ScriptLoader(destination);

            var factory = new MapFactory(Path.GetDirectoryName(path));
            var h2map = factory.Load(Path.GetFileName(path));

            if (h2map is not H2mccMap mccMap)
            {
                throw new NotSupportedException("Only MCC maps are supported in this tool");
            }

            var scnr = mccMap.Scenario;
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
                CollectBuiltins(text);
                var debugTree = text.ToString(Args.CreateVerboseTrees);
                File.WriteAllText(Path.Combine(debugRoot, script.Description + ".tree"), debugTree);
            }
        }

        private void CollectBuiltins(ScriptTreeNode root)
        {
            var nodes = new Stack<ScriptTreeNode>();

            nodes.Push(root);

            while(nodes.TryPop(out var node))
            {
                // Ignore per-scenario script method invocations
                if(node.Original.NodeType == Core.Scripting.NodeType.ScriptInvocation)
                {
                    continue;
                }

                if(node.Original.NodeType == Core.Scripting.NodeType.Expression &&
                    node.Original.DataType == Core.Scripting.ScriptDataType.MethodOrOperator)
                {
                    MethodInfos.AddOrUpdate(node.Value as string, k => new HashSet<ushort>() { node.Original.ScriptIndex }, (k, h) =>
                    {
                        h.Add(node.Original.ScriptIndex);
                        return h;
                    });
                }

                foreach(var grandChild in node.Children)
                    nodes.Push(grandChild);
            }
        }

        private string GenerateBuiltinInfo()
        {
            return string.Join("\r\n", this.MethodInfos.OrderBy(i => i.Value.First()).Select(i => JsonSerializer.Serialize(i.Value) + ": " + i.Key));
        }
    }
}
