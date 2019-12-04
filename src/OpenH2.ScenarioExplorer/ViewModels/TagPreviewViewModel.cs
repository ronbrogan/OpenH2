using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace OpenH2.ScenarioExplorer.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class TagPreviewViewModel
    {
        private Dictionary<string, object> resources = new Dictionary<string, object>();
        private Dictionary<string, Func<object, object>> transforms = new Dictionary<string, Func<object, object>>();

        private Dictionary<string, object> cachedTransformResults = new Dictionary<string, object>();

        private ObservableCollection<string> keys = new ObservableCollection<string>();

        public TagPreviewViewModel()
        {

        }

        public ObservableCollection<string> PreviewItems { get
            {
                return keys;
            }
        }

        public string SelectedPreviewItem { get; set; }

        public object SelectedPreviewItemObject => GetItem();

        public void AddItem<TState>(string key, TState value, Func<TState, object> previewTransform) where TState: class
        {
            resources.Add(key, value);
            transforms.Add(key, (object v) => previewTransform(v as TState));
            keys.Add(key);
        }

        public void AddItem(string key, object result)
        {
            keys.Add(key);
            cachedTransformResults[key] = result;
        }

        public object GetItem()
        {
            if (string.IsNullOrWhiteSpace(SelectedPreviewItem))
                SelectedPreviewItem = keys[0];

            if (cachedTransformResults.ContainsKey(SelectedPreviewItem))
            {
                return cachedTransformResults[SelectedPreviewItem];
            }

            var result = transforms[SelectedPreviewItem](resources[SelectedPreviewItem]);

            cachedTransformResults[SelectedPreviewItem] = result;

            return result;
        }
    }
}
