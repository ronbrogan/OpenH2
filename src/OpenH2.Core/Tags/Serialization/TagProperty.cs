using OpenH2.Core.Tags.Layout;
using OpenH2.Serialization.Layout;
using System;
using System.Reflection;

namespace OpenH2.Core.Tags.Serialization
{
    public class TagProperty
    {
        public SerializableMemberAttribute LayoutAttribute { get; set; }

        public Type Type { get; set; }

        public MethodInfo Setter { get; set; }

        public MethodInfo Getter { get; set; }
    }
}
