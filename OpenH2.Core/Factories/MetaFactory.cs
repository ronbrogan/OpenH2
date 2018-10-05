using OpenH2.Core.Parsing;
using OpenH2.Core.Processors.Meta;
using OpenH2.Core.Representations;
using OpenH2.Core.Representations.Meta;
using System.Collections.Generic;

namespace OpenH2.Core.Factories
{
    public static class MetaFactory
    {
        private delegate BaseMeta ProcessMeta(string name, ObjectIndexEntry entry, TrackingChunk chunk);
        private static readonly Dictionary<string, ProcessMeta> ProcessMap = new Dictionary<string, ProcessMeta>
        {
            { "bitm", BitmMetaProcessor.ProcessBitm },
            { "mode", ModelMetaProcessor.ProcessModel }
        };

        public static BaseMeta GetMeta(string name, ObjectIndexEntry index, TrackingChunk chunk)
        {
            if (ProcessMap.ContainsKey(index.Tag) == false)
                return null;

            return ProcessMap[index.Tag](name, index, chunk);
        }
    }
}
