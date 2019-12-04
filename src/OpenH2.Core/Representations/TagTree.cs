using System;
using OpenH2.Core.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace OpenH2.Core.Representations
{
    public class TagTree
    {
        private Dictionary<string, TagListEntry> children = new Dictionary<string, TagListEntry>();
        private TagListEntry root = new TagListEntry("root");

        private TagListEntry lookupChild(string key)
        {
            if (key.IsSignificant() == false)
                return null;

            if (children.TryGetValue(key, out var child))
                return child;
            else
                return null;
        }

        public TagListEntry Add(string tag, string parent = null, string grandparent = null)
        {
            var gp = lookupChild(grandparent);
            var p = lookupChild(parent);
            var c = lookupChild(tag);

            if(gp == null && grandparent.IsSignificant())
            {
                gp = new TagListEntry(grandparent);
                children.Add(grandparent, gp);
            }

            if (p == null && parent.IsSignificant())
            {
                p = new TagListEntry(parent);
                children.Add(parent, p);
            }

            if (c == null && tag.IsSignificant())
            {
                c = new TagListEntry(tag);
                children.Add(tag, c);
            }

            gp?.Add(p);

            if(p != null)
            {
                p.Add(c);
            }
            else
            {
                root.Add(c);
            }

            if(gp != null && gp.Parent == null)
            {
                root.Add(gp);
            }
            else if (p != null && p.Parent == null)
            {
                root.Add(p);
            }

            return c;
        }

        public IEnumerable<TagListEntry> Children()
        {
            return root.Children;
        }

        public void Print(Action<string> printLine)
        {
            foreach(var child in root.Children)
            {
                printImpl(printLine, child);
            }
        }

        private void printImpl(Action<string> printLine, TagListEntry current, int level = 0)
        {
            var indent = string.Join(string.Empty, Enumerable.Range(0, level).Select(_ => "\t"));
            printLine(indent + current.Tag);

            foreach (var child in current.Children)
            {
                printImpl(printLine, child, level + 1);
            }
        }
    }
}
