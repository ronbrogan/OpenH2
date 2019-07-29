using OpenH2.Core.Representations;
using OpenH2.Core.Tags;
using OpenH2.Translation.TagData;
using OpenH2.Translation.TagData.Processors;
using System.Collections.Generic;

namespace OpenH2.Translation
{
    public class TagTranslator
    {
        private readonly H2vMap scene;
        private readonly TagDataCache cache;

        public TagTranslator(H2vMap scene)
        {
            this.scene = scene;
            this.cache = new TagDataCache();

            foreach(var entry in scene.TagIndex)
            {
                // TODO: Tag can be null here because we don't process all tags yet
                if(scene.TryGetTag<BaseTag>(entry.ID, out var tag) && tag != null)
                {
                    this.AddTag(tag);
                }
            }
        }

        private void AddTag(BaseTag tag)
        {
            var data = TagDataFactory.CreateTagData(tag);

            if(data != null)
            {
                cache.AddTagData(data);
            }
        }

        public TTagData Get<TTagData>(uint key)
        {
            return cache.GetEntries<TTagData>()[key];
        }

        public IEnumerable<TTagData> GetAll<TTagData>()
        {
            return cache.GetEntries<TTagData>().Values;
        }

    }
}
