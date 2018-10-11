using OpenH2.Core.Meta;
using OpenH2.Core.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenH2.Core.Tags
{
    public class ModelTagNode : TagNode
    {
        public ModelMeta Meta { get; set; }

        public Memory<byte>[] RawPartData { get; set; }

        public Mesh[] Parts { get; set; }
    }
}
