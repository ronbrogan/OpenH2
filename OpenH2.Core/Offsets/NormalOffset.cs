using System;
using System.Collections.Generic;
using System.Text;

namespace OpenH2.Core.Offsets
{
    public class NormalOffset : IOffset
    {
        private int offset;

        public NormalOffset(int offset)
        {
            this.offset = offset;
        }

        public int Value => this.offset;

        public int OriginalValue => this.offset;
    }
}
