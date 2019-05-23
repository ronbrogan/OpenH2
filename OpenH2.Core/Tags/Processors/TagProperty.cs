using OpenH2.Core.Tags.Layout;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace OpenH2.Core.Tags.Processors
{
    public class TagProperty
    {
        public TagValueAttribute LayoutAttribute { get; set; }

        public Type Type { get; set; }

        public MethodInfo Setter { get; set; }

        public MethodInfo Getter { get; set; }
    }
}
