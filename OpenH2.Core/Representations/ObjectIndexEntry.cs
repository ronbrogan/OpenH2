using OpenH2.Core.Offsets;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenH2.Core.Representations
{
    public class ObjectIndexEntry
    {
        public string Tag { get; set; }
        public uint ID { get; set; }
        public SecondaryOffset Offset { get; set; }
        public int MetaSize { get; set; }

        public static int Size => 16;
    }
}
