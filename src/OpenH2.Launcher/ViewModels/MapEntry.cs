using OpenH2.Core.Factories;
using OpenH2.Core.Maps;
using PropertyChanged;
using System.IO;

namespace OpenH2.Launcher.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class MapEntry
    {
        public string FullPath { get; set; }

        public string FileName { get; set; }

        public string FriendlyName { get; set; }

        public IH2MapInfo InformationalMap { get; set; }

        public MapEntry(string fullPath)
        {
            this.FullPath = fullPath;
            this.FileName = Path.GetFileName(fullPath);

            this.InformationalMap = MapFactory.LoadInformational(fullPath);
            this.FriendlyName = this.InformationalMap.Name;
        }
    }
}
