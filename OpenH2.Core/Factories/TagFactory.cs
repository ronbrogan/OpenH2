using OpenH2.Core.Meta;
using OpenH2.Core.Parsing;
using OpenH2.Core.Tags;
using OpenH2.Core.Tags.Processors;
using System;
using System.Collections.Generic;

namespace OpenH2.Core.Factories
{
    public static class TagFactory
    {
        private delegate TagNode ProcessMeta(BaseMeta meta, TrackingReader reader);
        private static Dictionary<Type, ProcessMeta> Processors = new Dictionary<Type, ProcessMeta>
        {
            { typeof(BitmapMeta),   BitmapTagProcessor.ProcessBitmapMeta },
            { typeof(ModelMeta),    ModelTagProcessor.ProcessModelMeta }
        };

        public static TagNode CreateTag(BaseMeta meta, TrackingReader reader)
        {
            var metaType = meta.GetType();

            if (Processors.ContainsKey(metaType) == false)
                return null;

            return Processors[metaType](meta, reader);
        }
    }
}
