using OpenH2.Core.Representations;
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

        public void PopulateChildren(TagViewModel vm, TagTreeEntryViewModel entry)
        {
            if (scene.TryGetTag<BaseTag>(entry.Id, out var tag) == false)
            {
                return;
            }

            var childRefs = GetChildReferences(tag);

            var childrenVms = new List<TagTreeEntryViewModel>();

            foreach (var child in childRefs)
            {
                if (scene.TryGetTag<BaseTag>(child.Id, out var childTag) == false)
                    continue;

                if (childTag == null)
                {
                    var indexEntry = scene.TagIndex[child.Id];
                    Console.WriteLine($"Found null tag for [{indexEntry.Tag}] tag");

                    childrenVms.Add(new TagTreeEntryViewModel()
                    {
                        Id = child.Id,
                        TagName = indexEntry.Tag.ToString()
                    });

                    continue;
                }

                var tagLabel = childTag.GetType().GetCustomAttribute<TagLabelAttribute>().Label;
                var tagName = tagLabel + (childTag.Name != null ? " - " + childTag.Name : string.Empty);

                childrenVms.Add(new TagTreeEntryViewModel()
                {
                    Id = child.Id,
                    TagName = tagName
                });
            }

            entry.Children = childrenVms.ToArray();
        }

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

                var currentProps = current.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

                var tagRefProps = currentProps.Where(t => (t.PropertyType.IsGenericType && t.PropertyType.GetGenericTypeDefinition() == typeof(TagRef<>))
                        || t.PropertyType == typeof(TagRef))
                    .ToList();

                foreach (var prop in tagRefProps)
                {
                    props.Add((current, prop));
                }

                var nestedArrays = currentProps.Where(t => (t.PropertyType.IsArray && t.PropertyType.GetElementType().IsClass));

                foreach (var nested in nestedArrays)
                {
                    var arr = (object[])nested.GetValue(current);

                    if (arr == null)
                        continue;

                    foreach (var arrItem in arr)
                    {
                        CollectRecursiveTagRefs(arrItem, props);
                    }
                }

                var nestedItems = currentProps.Where(t => t.PropertyType.IsClass);

                foreach (var nested in nestedItems)
                {
                    CollectRecursiveTagRefs(nested.GetValue(current), props);
                }
            }
        }
    }
}
