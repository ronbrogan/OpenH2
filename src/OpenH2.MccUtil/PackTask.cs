using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenH2.MccUtil
{
    

    public class PackCommandLineArguments
    {
        [Option('f', "files", Required = true, HelpText = "File glob")]
        public string Files { get; set; }

        [Option("suppress-signature", HelpText = "Don't sign when re-compressing")]
        public bool SuppressSignature { get; set; }
    }
}
