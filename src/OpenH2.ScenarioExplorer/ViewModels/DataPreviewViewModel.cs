using Avalonia.Media;
using OpenBlam.Core.Extensions;
using OpenH2.Core.Extensions;
using PropertyChanged;
using System;
using System.Numerics;

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
            this.Vec3 = data.ReadVec3At(offset);

            try
            {
                var colorHex = $"#{ColorHex(Vec3.X)}{ColorHex(Vec3.Y)}{ColorHex(Vec3.Z)}";
                this.ColorBrush = (ISolidColorBrush)new BrushConverter().ConvertFromString(colorHex);
            }
            catch { }
            

            this.InternalOffset = (long)this.UInt - tag.InternalOffsetStart;
            this.SelectedOffset = tag.RawOffset + offset;

            string ColorHex(float component)
            {
                return Convert.ToString((int)(Math.Clamp(component, 0f, 1f) * 255f), 16).PadLeft(2, '0');
            }
        }

        public byte Byte { get; set; }

        public short Short { get; set; }

        public ushort UShort { get; set; }

        public int Int { get; set; }

        public uint UInt { get; set; }

        public float Float { get; set; }

        public Vector3 Vec3 { get; set; }
        public ISolidColorBrush ColorBrush { get; private set; }
        public string String { get; set; }

        public string InternedString { get; set; }
        public string TagName { get; set; }

        public long InternalOffset { get; set; }
        public string FileOffset { get; set; }

        public long SelectedOffset { get; set; }

        public void CopyData(string data)
        {
            App.Current.Clipboard.SetTextAsync(data);
        }
    }
}
