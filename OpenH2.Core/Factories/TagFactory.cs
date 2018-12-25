using OpenH2.Core.Tags;
using OpenH2.Core.Tags.Processors;
using OpenH2.Core.Parsing;
using OpenH2.Core.Representations;
using System.Collections.Generic;

namespace OpenH2.Core.Factories
{
    public static class TagFactory
    {
        private delegate BaseTag ProcessMeta(uint id, string name, TagIndexEntry entry, TrackingChunk chunk, TrackingReader sceneReader);
        private static readonly Dictionary<string, ProcessMeta> ProcessMap = new Dictionary<string, ProcessMeta>
        {
            { "bitm", BitmapTagProcessor.ProcessBitm },
            { "mode", ModelTagProcessor.ProcessModel },
            { "sbsp", BspTagProcessor.ProcessBsp },
            { "scnr", ScenarioTagProcessor.ProcessScenario }
        };

        public static BaseTag CreateTag(uint id, string name, TagIndexEntry index, TrackingChunk chunk, TrackingReader sceneReader)
        {
            if (ProcessMap.ContainsKey(index.Tag) == false)
                return null;

            return ProcessMap[index.Tag](id, name, index, chunk, sceneReader);
        }
    }
}
