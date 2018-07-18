using System;

namespace OpenH2.Core.Tags
{
    public class BitmapTagNode : TagNode
    {
        public Span<byte> Data { get; set; }

        public bool Swizzled {get;set;}

        // Mip maps stored as child bitmap tag nodes?
        public bool MipMapped { get; set; }
        
        public int LevelOfDetail { get; set; }
    }
}
