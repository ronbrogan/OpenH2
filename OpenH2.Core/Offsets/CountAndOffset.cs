﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace OpenH2.Core.Offsets
{
    [DebuggerDisplay("{Count} @ {Offset.Value}")]
    public class CountAndOffset
    {
        public CountAndOffset(int Count, IOffset Offset)
        {
            this.Count = Count;
            this.Offset = Offset;
        }

        public IOffset Offset { get; set; }

        public int Count { get; set; }
    }
}