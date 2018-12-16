using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace OpenH2.Core.Representations
{
    [DebuggerDisplay("{Tag}")]
    public class TagListEntry
    {
        public TagListEntry Parent { get; set; }

        public HashSet<TagListEntry> Children = new HashSet<TagListEntry>();

        public string Tag { get; private set; }

        public TagListEntry(string tag)
        {
            this.Tag = tag;
        }

        internal void Add(TagListEntry entry)
        {
            if(entry.Parent != null)
            {
                entry.Parent.Remove(entry);
            }

            Children.Add(entry);
            entry.Parent = this;
        }

        public void Remove(TagListEntry entry)
        {
            this.Children.Remove(entry);
            entry.Parent = null;
        }
    }
}