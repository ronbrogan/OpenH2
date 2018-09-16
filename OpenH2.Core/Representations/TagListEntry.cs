using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace OpenH2.Core.Representations
{
    [DebuggerDisplay("{Class} <- {ParentClass} <- {GrandparentClass}")]
    public class TagListEntry
    {
        public string Class { get; set; }
        public string ParentClass { get; set; }
        public string GrandparentClass { get; set; }
    }
}
