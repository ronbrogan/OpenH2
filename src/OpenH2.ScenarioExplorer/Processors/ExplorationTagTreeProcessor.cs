using OpenH2.Core.Maps;
using OpenH2.Core.Tags;
using OpenH2.Core.Tags.Layout;
using OpenH2.ScenarioExplorer.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OpenH2.ScenarioExplorer.Processors
{
    public class ExplorationTagTreeProcessor : ITagTreeProcessor
    {
        private readonly H2vMap scene;

        public ExplorationTagTreeProcessor(H2vMap scene)
        {
            this.scene = scene;
        }

        public Dictionary<Type, string> TagLabels = new Dictionary<Type, string>();

        public void PopulateChildren(TagViewModel vm, TagTreeEntryViewModel entry)
        {
            if (scene.TryGetTag<BaseTag>(entry.Id, out var tag) == false)
            {
                return;
            }

            var childRefs = GetChildReferences(tag);

            var addedChildren = new HashSet<uint>();
            var childrenVms = new List<TagTreeEntryViewModel>();


            foreach (var child in childRefs)
            {
                if (addedChildren.Contains(child.Id))
                    continue;

                if (scene.TryGetTag<BaseTag>(child.Id, out var childTag) == false)
                    continue;

                if (childTag == null)
                {
                    if(scene.TagIndex.TryGetValue(child.Id, out var indexEntry) == false)
                    {
                        Console.WriteLine($"Unable to find any tag entry info for ????[{child.Id}]");
                        continue;
                    }

                    Console.WriteLine($"Found null tag for [{indexEntry.Tag}] tag");

                    addedChildren.Add(child.Id);
                    childrenVms.Add(new TagTreeEntryViewModel(childTag));

                    continue;
                }

                string tagLabel = "";

                if(childTag is UnknownTag uk)
                {
                    tagLabel = uk.OriginalLabel + "[?]";
                }
                else
                {
                    if(TagLabels.TryGetValue(childTag.GetType(), out tagLabel) == false)
                    {
                        tagLabel = childTag.GetType().GetCustomAttribute<TagLabelAttribute>().Label.ToString();
                        TagLabels.Add(childTag.GetType(), tagLabel);
                    }
                }


                addedChildren.Add(child.Id);
                childrenVms.Add(new TagTreeEntryViewModel(childTag));
            }

            entry.Children = childrenVms.ToArray();
        }

        private Dictionary<Type, PropertyInfo[]> TagReferencePropertyCache = new Dictionary<Type, PropertyInfo[]>();
        private Dictionary<Type, PropertyInfo[]> TagReferenceArrayPropertyCache = new Dictionary<Type, PropertyInfo[]>();
        private Dictionary<Type, PropertyInfo[]> TagReferenceObjectPropertyCache = new Dictionary<Type, PropertyInfo[]>();

        // TODO: This is likely overeager
        private ITagRef[] GetChildReferences(BaseTag tag)
        {
            var refProps = new List<(object, PropertyInfo)>();
            CollectRecursiveTagRefs(tag, refProps);

            var refs = new ITagRef[refProps.Count];

            for (var i = 0; i < refProps.Count; i++)
            {
                var (obj, prop) = refProps[i];

                var val = prop.GetValue(obj);

                refs[i] = (ITagRef)val;
            }

            return refs;

            void CollectRecursiveTagRefs(object current, List<(object, PropertyInfo)> props)
            {
                if (current == null)
                    return;

                var currType = current.GetType();

                TagReferencePropertyCache.TryGetValue(currType, out var tagRefProps);
                TagReferenceArrayPropertyCache.TryGetValue(currType, out var tagRefArrayProps);
                TagReferenceObjectPropertyCache.TryGetValue(currType, out var tagRefObjectProps);

                PropertyInfo[] allProperties = null;
                if ((tagRefProps ?? tagRefArrayProps ?? tagRefObjectProps) == null)
                {
                    allProperties = current.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                }

                if(tagRefProps == null)
                {
                    tagRefProps = allProperties.Where(t => (t.PropertyType.IsGenericType && t.PropertyType.GetGenericTypeDefinition() == typeof(TagRef<>))
                        || t.PropertyType == typeof(TagRef))
                    .ToArray();
                    TagReferencePropertyCache.Add(currType, tagRefProps);
                }
                
                foreach (var prop in tagRefProps)
                {
                    props.Add((current, prop));
                }

                if(tagRefArrayProps == null)
                {
                    tagRefArrayProps = allProperties.Where(t => (t.PropertyType.IsArray && t.PropertyType.GetElementType().IsClass)).ToArray();
                    TagReferenceArrayPropertyCache.Add(currType, tagRefArrayProps);
                }

                foreach (var nested in tagRefArrayProps)
                {
                    var arr = (object[])nested.GetValue(current);

                    if (arr == null)
                        continue;

                    foreach (var arrItem in arr)
                    {
                        CollectRecursiveTagRefs(arrItem, props);
                    }
                }

                if (tagRefObjectProps == null)
                {
                    tagRefObjectProps = allProperties.Where(t => t.PropertyType.IsClass).ToArray();
                    TagReferenceObjectPropertyCache.Add(currType, tagRefObjectProps);
                }

                foreach (var nested in tagRefObjectProps)
                {
                    try
                    {
                        CollectRecursiveTagRefs(nested.GetValue(current), props);
                    }
                    catch { }
                }
            }
        }
    }
}
