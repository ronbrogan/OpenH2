using OpenH2.Core.Tags;
using System;

namespace OpenH2.Translation.TagData
{
    public class BitmapTagData : BaseTagData
    {
        public BitmapTagData(Bitmap tag) : base(tag)
        {
            Levels = new Memory<byte>[tag.LevelsOfDetail.Length];
            Name = tag.Name;
        }

        public string Name { get; set; }

        public Memory<byte>[] Levels { get; set; }
    }
}