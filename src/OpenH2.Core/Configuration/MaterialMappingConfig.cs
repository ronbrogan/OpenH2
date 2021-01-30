using System;
using System.Collections.Generic;
using System.Text;

namespace OpenH2.Core.Configuration
{
    public class MaterialMappingConfig
    {
        public Dictionary<string, MaterialAlias> Aliases { get; set; }
        public Dictionary<string, MaterialMapping> Mappings { get; set; }
    }
}
