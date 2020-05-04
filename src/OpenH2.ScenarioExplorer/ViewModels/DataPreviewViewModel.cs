using OpenH2.Core.Extensions;
using PropertyChanged;

namespace OpenH2.ScenarioExplorer.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class DataPreviewViewModel
    {
        public DataPreviewViewModel(int offset, TagViewModel tag)
        {
            var data = tag.Data.Span;

            if (data.Length == 0)
                return;

            if (offset < 0 || offset > data.Length - 1)
                return;

            this.Byte = data[offset];
            this.Short = data.ReadInt16At(offset);
            this.UShort = data.ReadUInt16At(offset);
            this.Int = data.ReadInt32At(offset);
            this.UInt = data.ReadUInt32At(offset);
            this.Float = data.ReadFloatAt(offset);
            this.String = data.ReadStringFrom(offset, 32);

            this.InternalOffset = (long)data.ReadUInt32At(offset) - tag.InternalOffsetStart;
            this.SelectedOffset = tag.RawOffset + offset;
        }

        public byte Byte { get; set; }

        public short Short { get; set; }

        public ushort UShort { get; set; }

        public int Int { get; set; }

        public uint UInt { get; set; }

        public float Float { get; set; }

        public string String { get; set; }

        public string InternedString { get; set; }

        public long InternalOffset { get; set; }

        public long SelectedOffset { get; set; }

        public void CopyData(string data)
        {
            App.Current.Clipboard.SetTextAsync(data);
        }
    }
}
