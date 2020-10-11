using OpenH2.Core.Factories;
using OpenH2.Core.Scripting.Generation;
using OpenH2.Core.Scripting.LowLevel;
using OpenH2.Core.Tags.Scenario;
using System;
using System.IO;
using System.Linq;

namespace OpenH2.ScriptAnalysis
{
    public class Program
    {
        static void Main(string[] args)
        {
            var outRoot = $@"D:\h2scratch\scripts";
            Directory.CreateDirectory(outRoot);

            var loader = new ScriptLoader(outRoot);
            var maps = Directory.GetFiles(@"D:\H2vMaps", "*.map");

            foreach (var map in maps)
            {
                if (map.Contains("0") == false)
                {
                    continue;
                }

                var factory = new MapFactory(Path.GetDirectoryName(map), NullMaterialFactory.Instance);
                var scene = factory.FromFile(File.OpenRead(map));

                var scnr = scene.GetLocalTagsOfType<ScenarioTag>().First();
                loader.Load(scnr);

                var scenarioParts = scnr.Name.Split('\\', StringSplitOptions.RemoveEmptyEntries)
                        .Select(p => p.Trim())
                        .ToArray();

                var debugRoot = $@"{outRoot}\{scenarioParts.Last()}";
                Directory.CreateDirectory(debugRoot);

                for (int i = 0; i < scnr.ScriptMethods.Length; i++)
                {
                    var script = scnr.ScriptMethods[i];
                    var text = ScriptProcessor.GetScriptTree(scnr, script, i);
                    var debugTree = text.ToString(verbose: true);
                    File.WriteAllText(Path.Combine(debugRoot, script.Description + ".tree"), debugTree);
                }
            }
        }
    }
}
