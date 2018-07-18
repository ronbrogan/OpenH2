using System;
using System.Collections.Generic;
using System.Text;

namespace OpenH2.Core.Representations
{
    public class SceneMetadata
    {
        public string Name { get; set; }

        public long CalculatedSignature { get; set; }

        public long StoredSignature { get; set; }
    }
}
