using CommandLine;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using OpenH2.Core.Maps.MCC;
using OpenH2.Serialization;
using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace OpenH2.MccUtil
{
    [Verb("unpack")]
    public class UnpackCommandLineArguments
    {
        [Option('d', "directory", HelpText = "Working directory to search for files under")]
        public string WorkingDirectory { get; set; }

        [Option('f', "files", Required = true, HelpText = "File glob")]
        public string Files { get; set; }

        [Option('o', "output", HelpText = "Specify output directory of unpacked map")]
        public string OutputDirectory { get; set; } = @"D:\h2scratch\mcc";

        [Option('s', "suffix", HelpText = "Make a copy with the specified suffix")]
        public string CopySuffix { get; set; } = "unpacked";
    }

    public class UnpackTask
    {
        private readonly UnpackCommandLineArguments args;

        public static async Task Run(UnpackCommandLineArguments args)
        {
            var t = new UnpackTask(args);
            t.Run();
        }

        public UnpackTask(UnpackCommandLineArguments args)
        {
            this.args = args;
        }

        public void Run()
        {
            var matcher = new Matcher();

            matcher.AddInclude(args.Files);

            var root = new DirectoryInfoWrapper(new DirectoryInfo(this.args.WorkingDirectory ?? Environment.CurrentDirectory));

            Console.WriteLine($"Looking for files matching '{args.Files}' in '{root.FullName}'");

            var results = matcher.Execute(root);

            if(results.HasMatches == false)
            {
                Console.WriteLine("No matching files found");
                return;
            }

            if(this.args.OutputDirectory != null)
            {
                Directory.CreateDirectory(this.args.OutputDirectory);
            }

            foreach(var result in results.Files)
            {
                Console.WriteLine($"Found '{result.Path}', unpacking...");

                Unpack(Path.Combine(root.FullName, result.Path));
            }
        }

        public void Unpack(string path)
        {
            using var mapIn = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var mapOutPath = Path.Combine(this.args.OutputDirectory ?? Path.GetDirectoryName(path),
                $"{Path.GetFileNameWithoutExtension(path)}-{this.args.CopySuffix}.map");
            using var mapOut = new FileStream(mapOutPath, FileMode.Create);

            var info = BlamSerializer.Deserialize<H2mccCompressionSections>(mapIn);

            var header = new Span<byte>(new byte[4096]);

            mapIn.Seek(0, SeekOrigin.Begin);
            mapIn.Read(header);
            mapOut.Write(header);

            foreach(var section in info.Sections)
            {
                if (section.Offset == 0 || section.Count == 0)
                    continue;

                var start = mapOut.Position;

                mapIn.Seek(section.Offset+2, SeekOrigin.Begin);
                using var deflate = new DeflateStream(mapIn, CompressionMode.Decompress, leaveOpen: true);
                deflate.CopyTo(mapOut);

                Console.WriteLine($"Decompressed {mapOut.Position - start}b chunk");
            }
        }
    }
}
