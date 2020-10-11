using CommandLine;
using OpenH2.Core.Extensions;
using OpenH2.Core.Maps.MCC;
using OpenH2.Serialization;
using OpenH2.Serialization.Layout;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OpenH2.MccUtil
{
    
    [Verb("pack")]
    public class PackCommandLineArguments
    {
        [Option('f', "file", Required = true, HelpText = "File to pack")]
        public string FilePath { get; set; }

        [Option('o', "out-file", Required = true, HelpText = "Destination of packed file")]
        public string OutPath { get; set; }
    }

    public class PackTask
    {
        private readonly PackCommandLineArguments args;

        public static async Task Run(PackCommandLineArguments args)
        {
            var t = new PackTask(args);
            t.Run();
        }

        public PackTask(PackCommandLineArguments args)
        {
            this.args = args;
        }

        public void Run()
        {
            using var map = new FileStream(this.args.FilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.Read, 80192, false);

            var sig = H2mccMap.CalculateSignature(map);
            
            var sigOffset = BlamSerializer.StartsAt<H2mccMapHeader>(h => h.StoredSignature);

            map.WriteUInt32At(sigOffset, (uint)sig);
            map.Seek(0, SeekOrigin.Begin);

            // Create destination map on disk
            using var outMap = File.OpenWrite(this.args.OutPath);

            H2mccCompression.Compress(map, outMap);
        }
    }
}
