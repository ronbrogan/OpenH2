using OpenH2.Core.Extensions;
using OpenH2.Core.Maps.MCC;
using OpenH2.Core.Tags.Scenario;
using OpenH2.Serialization;
using System;
using System.IO;

namespace OpenH2.Core.Scripting.LowLevel
{
    public static class ScriptTreePatcher
    {
        public static void PatchMap(H2mccMap scene, Stream map, string patchFilePath)
        {
            var scenarioStart = scene.TagIndex[scene.IndexHeader.Scenario].Offset.Value;
            var nodesStart = BlamSerializer.StartsAt<ScenarioTag>(s => s.ScriptSyntaxNodes);
            var nodeCount = map.ReadUInt32At(scenarioStart + nodesStart);
            var nodeOffset = (int)map.ReadUInt32At(scenarioStart + nodesStart + 4) - scene.SecondaryMagic;

            var nodeSize = BlamSerializer.SizeOf<ScenarioTag.ScriptSyntaxNode>();

            var patchLines = File.ReadAllLines(patchFilePath);

            foreach (var line in patchLines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                if (ShouldPatchFrom(scene, line, out var patch))
                {
                    Console.WriteLine($"\t Patching {scene.Header.Name} [{patch.Index}]");
                    var patchStart = nodeOffset + patch.Index * nodeSize;

                    // Fixup next node's check value. We never change check values, so we can
                    // re-use the 'old' nodes here to get that info
                    if(patch.NodeData.NextIndex != ushort.MaxValue)
                    {
                        var nextNode = scene.Scenario.ScriptSyntaxNodes[patch.NodeData.NextIndex];
                        patch.NodeData.NextCheckval = nextNode.Checkval;
                    }

                    // Fixup next node's check value for scope/invocation nodes
                    if((patch.NodeData.NodeType == NodeType.Scope || patch.NodeData.NodeType == NodeType.ScriptInvocation)
                        && patch.NodeData.NodeData_H16 != ushort.MaxValue)
                    {
                        var nextNode = scene.Scenario.ScriptSyntaxNodes[patch.NodeData.NodeData_H16];
                        patch.NodeData.NodeData_32 = patch.NodeData.NodeData_H16 | ((uint)nextNode.Checkval) << 16;
                    }

                    //map.WriteUInt16At(patchStart + 0, patch.NodeData.Checkval);
                    map.WriteUInt16At(patchStart + 2, patch.NodeData.ScriptIndex);
                    map.WriteUInt16At(patchStart + 4, (ushort)patch.NodeData.DataType);
                    map.WriteUInt16At(patchStart + 6, (ushort)patch.NodeData.NodeType);
                    map.WriteUInt16At(patchStart + 8, patch.NodeData.NextIndex);
                    map.WriteUInt16At(patchStart + 10, patch.NodeData.NextCheckval);
                    map.WriteUInt16At(patchStart + 12, patch.NodeData.NodeString);
                    //map.WriteUInt16At(patchStart + 14, patch.NodeData.ValueH);
                    map.WriteUInt32At(patchStart + 16, patch.NodeData.NodeData_32);
                }
            }
        }

        public static bool ShouldPatchFrom(H2mccMap scene, string line, out SyntaxNodePatch patch)
        {
            patch = null;

            if (line.TrimStart().StartsWith("//"))
            {
                return false;
            }

            try
            {
                patch = new SyntaxNodePatch
                {
                    Index = ParseFrom(line, "@:", " "),
                    NodeData = new ScenarioTag.ScriptSyntaxNode
                    {
                        //Checkval = ParseFrom(line, "a:"),
                        ScriptIndex = ParseFrom(line, "op:"),
                        DataType = (ScriptDataType)ParseFrom(line, "dt:"),
                        NodeType = (NodeType)ParseFrom(line, "nt:"),
                        NextIndex = ParseFrom(line, "next:"),
                        //NextCheckval = ParseFrom(line, "f:"),
                        NodeString = ParseFrom(line, "str:"),
                        //ValueH = ParseFrom(line, "h:"),
                        NodeData_32 = ParseFrom(line, "d0:") | ((uint)ParseFrom(line, "d1:", ";")) << 16
                    }
                };

                if (patch.NodeData.NodeType == NodeType.MethodDecl)
                {
                    return false;
                }

                var node = scene.Scenario.ScriptSyntaxNodes[patch.Index];

                if (node.ScriptIndex != patch.NodeData.ScriptIndex
                    || node.DataType != patch.NodeData.DataType
                    || node.NodeType != patch.NodeData.NodeType
                    || node.NextIndex != patch.NodeData.NextIndex
                    || node.NodeString != patch.NodeData.NodeString
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
            var start = data.IndexOf(prefix) + prefix.Length;
            var end = data.Length;

            if (suffix != null)
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
