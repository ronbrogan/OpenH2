using OpenH2.Core.Offsets;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenH2.Core.Representations
{
    public class SceneHeader
    {
        public string FileHead { get; set; }
        public int Version { get; set; }
        public int TotalBytes { get; set; }
        // This property is apparently always zero
        public int Zero => 0;
        public NormalOffset IndexOffset { get; set; }
        public PrimaryOffset MetaOffset { get; set; }

        // 2 dwords of unknown

        public string MapOrigin { get; set; }

        // 224 bytes of unknown

        public string Build { get; set; }

        // 20 bytes of unknown
        // int of unknown

        public int OffsetToStrangeFileStrings { get; set; }

        // int of unknown

        public int OffsetToUnknownSection { get; set; }
        public int ScriptReferenceCount { get; set; }
        public int SizeOfScriptReference { get; set; }
        public int OffsetToScriptReferenceIndex { get; set; }
        public int OffsetToScriptReferenceStrings { get; set; }
        
        // 36 bytes of unknown
        
        public string Name { get; set; }
        public int Zero2 => 0;

        // int of unknown

        public string ScenarioPath { get; set; }

        // 224 bytes of zero

        public int FileCount { get; set; }
        public int FileTableOffset { get; set; }
        public int FileTableSize { get; set; }
        public int FilesIndex { get; set; }
        

        //public int CalculatedSignature { get; set; }

        public int StoredSignature { get; set; }

        // 1320 bytes of zero

        public string Footer { get; set; }
    }
}
