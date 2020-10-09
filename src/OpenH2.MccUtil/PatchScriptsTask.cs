using CommandLine;
using OpenH2.Core.Extensions;
using OpenH2.Core.Factories;
using OpenH2.Core.Maps.MCC;
using OpenH2.Core.Scripting;
using OpenH2.Core.Scripting.Generation;
using OpenH2.Core.Tags.Scenario;
using OpenH2.ScriptAnalysis;
using OpenH2.Serialization;
using OpenH2.Serialization.Layout;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OpenH2.MccUtil
{
    [Verb("patch-scripts")]
    public class PatchScriptsCommandLineArguments
    {
        [Option('s', "script", Required = true, HelpText = "The script patch to load")]
        public string ScriptPatchPath { get; set; }

        [Option('m', "map", HelpText = "The map to apply the patch to")]
        public string MapPath { get; set; }
    }


    public class PatchScriptsTask
    {
        private H2mccMap scene;

        public PatchScriptsCommandLineArguments Args { get; }

        public static async Task Run(PatchScriptsCommandLineArguments args)
        {
            await new PatchScriptsTask(args).Run();
        }

        public PatchScriptsTask(PatchScriptsCommandLineArguments args)
        {
            this.Args = args;
        }

        public async Task Run()
        {
            // TODO: check if map is decompressed already?
            // Load to determine where to write patches to
            var factory = new MccMapFactory();
            this.scene = factory.FromFile(File.OpenRead(this.Args.MapPath));

            using var map = File.Open(this.Args.MapPath, FileMode.Open);

            var scenarioStart = scene.TagIndex[scene.IndexHeader.Scenario].Offset.Value;

            var nodesProp = typeof(ScenarioTag).GetProperty(nameof(ScenarioTag.ScriptSyntaxNodes));
            var refArrAttr = nodesProp.GetCustomAttribute<ReferenceArrayAttribute>();
            var nodeCount = map.ReadUInt32At(scenarioStart + refArrAttr.Offset);
            var nodeOffset = (int)map.ReadUInt32At(scenarioStart + refArrAttr.Offset + 4) - scene.SecondaryMagic;

            var nodeSize = BlamSerializer.SizeOf<ScenarioTag.ScriptSyntaxNode>();

            var patchLines = File.ReadAllLines(this.Args.ScriptPatchPath);

            foreach (var line in patchLines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                if(ShouldPatchFrom(line, out var patch))
                {
                    Console.WriteLine($"Patching node at '{patch.Index}'");
                    var patchStart = nodeOffset + patch.Index * nodeSize;

                    map.WriteUInt16At(patchStart + 0, patch.NodeData.Checkval);
                    map.WriteUInt16At(patchStart + 2, patch.NodeData.ScriptIndex);
                    map.WriteUInt16At(patchStart + 4, (ushort)patch.NodeData.DataType);
                    map.WriteUInt16At(patchStart + 6, (ushort)patch.NodeData.NodeType);
                    map.WriteUInt16At(patchStart + 8, patch.NodeData.NextIndex);
                    map.WriteUInt16At(patchStart + 10, patch.NodeData.NextCheckval);
                    map.WriteUInt16At(patchStart + 12, patch.NodeData.NodeString);
                    map.WriteUInt16At(patchStart + 14, patch.NodeData.ValueH);
                    map.WriteUInt32At(patchStart + 16, patch.NodeData.NodeData_32);
                }
            }
        }

        private bool ShouldPatchFrom(string line, out SyntaxNodePatch patch)
        {
            patch = null;

            try
            {
                patch = new SyntaxNodePatch
                {
                    Index = ParseFrom(line, "@:", " "),
                    NodeData = new ScenarioTag.ScriptSyntaxNode
                    {
                        Checkval = ParseFrom(line, "a:"),
                        ScriptIndex = ParseFrom(line, "b:"),
                        DataType = (ScriptDataType)ParseFrom(line, "c:"),
                        NodeType = (NodeType)ParseFrom(line, "d:"),
                        NextIndex = ParseFrom(line, "e:"),
                        NextCheckval = ParseFrom(line, "f:"),
                        NodeString = ParseFrom(line, "g:"),
                        ValueH = ParseFrom(line, "h:"),
                        NodeData_32 = ParseFrom(line, "i:") | ((uint)ParseFrom(line, "j:", null)) << 16
                    }
                };

                // TODO: better method line exclusion
                if(patch.Index == 0 && patch.NodeData.Checkval == 0)
                {
                    return false;
                }

                var node = this.scene.Scenario.ScriptSyntaxNodes[patch.Index];

                if(node.Checkval != patch.NodeData.Checkval
                    || node.ScriptIndex != patch.NodeData.ScriptIndex
                    || node.DataType != patch.NodeData.DataType
                    || node.NodeType != patch.NodeData.NodeType
                    || node.NextIndex != patch.NodeData.NextIndex
                    || node.NextCheckval != patch.NodeData.NextCheckval
                    || node.NodeString != patch.NodeData.NodeString
                    || node.ValueH != patch.NodeData.ValueH
                    || node.NodeData_32 != patch.NodeData.NodeData_32)
                {
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        private static ushort ParseFrom(string data, string prefix, string suffix = ",")
        {
            var start = data.IndexOf(prefix)+prefix.Length;
            var end = data.Length;

            if(suffix != null)
            {
                end = data.IndexOf(suffix, start);
            }

            return ushort.Parse(data.Substring(start, end - start));
        }

        public class SyntaxNodePatch
        {
            public ushort Index { get; set; }

            public ScenarioTag.ScriptSyntaxNode NodeData { get; set; }

        }
    }
}
