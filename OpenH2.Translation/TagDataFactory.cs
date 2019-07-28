using System;
using OpenH2.Core.Tags;
using OpenH2.Translation.TagData;
using OpenH2.Translation.TagData.Processors;
using System.Collections.Generic;

namespace OpenH2.Translation
{
    public static class TagDataFactory
    {
        private delegate BaseTagData ProcessMeta(BaseTag meta);
        private static Dictionary<Type, ProcessMeta> Translators = new Dictionary<Type, ProcessMeta>
        {
            { typeof(Model), ModelTagDataProcessor.ProcessTag },
            { typeof(Bsp), BspTagDataProcessor.ProcessTag }
        };

        private static ProcessMeta GetTranslator(Type tagType)
        {
            if(Translators.TryGetValue(tagType, out var translator))
            {
                return translator;
            }

            return null;
        }

        public static BaseTagData CreateTagData(BaseTag tag)
        {
            var translator = GetTranslator(tag.GetType());

            return translator?.Invoke(tag);
        }
    }
}
