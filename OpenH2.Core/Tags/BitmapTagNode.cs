using OpenH2.Core.Representations.Meta;
using System;

namespace OpenH2.Core.Tags
{
    public class BitmapTagNode : TagNode
    {
        public BitmapMeta Meta { get; set; }

        public Memory<byte>[] Levels { get; set; }
    }
}