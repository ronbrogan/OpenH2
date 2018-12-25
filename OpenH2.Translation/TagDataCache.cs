using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using OpenH2.Core.Tags;
using OpenH2.Translation.TagData;

namespace OpenH2.Translation
{
    public class TagDataCache
    {
        private Dictionary<uint, object> tagData = new Dictionary<uint, object>();

        private Dictionary<Type, IDictionary> tagDataByType = new Dictionary<Type, IDictionary>();

        public void AddTagData(BaseTagData data)
        {
            tagData[data.Id] = data;

            var type = data.GetType();

            if (tagDataByType.ContainsKey(type) == false)
            {
                var dictionaryType = typeof(Dictionary<,>).MakeGenericType(typeof(uint), type);

                tagDataByType[type] = (IDictionary)Activator.CreateInstance(dictionaryType);
            }
            
            tagDataByType[type][data.Id] = data;
            
        }

        public Dictionary<uint, TTagData> GetEntries<TTagData>()
        {
            
            if(tagDataByType.TryGetValue(typeof(TTagData), out var entries))
            {
                return (Dictionary<uint, TTagData>)entries;
            }
            else
            {
                return new Dictionary<uint, TTagData>();
            }
        }
    }
}
