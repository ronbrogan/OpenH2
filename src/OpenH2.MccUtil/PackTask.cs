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

        [Option("suppress-signature", HelpText = "Don't sign when re-compressing")]
        public bool SuppressSignature { get; set; } = false;
    }

    public class PackTask
    {
        public const int CompressionChunkSize = 2 << 17;

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
            using var map = File.OpenRead(this.args.FilePath);

            // Create signed map in-memory
            var signedMap = new MemoryStream();

            map.CopyTo(signedMap);
            signedMap.Seek(0, SeekOrigin.Begin);

            var sig = H2mccMap.CalculateSignature(signedMap.ToArray());
            var nodesProp = typeof(H2mccMapHeader).GetProperty(nameof(H2mccMapHeader.StoredSignature));
            var valAttr = nodesProp.GetCustomAttribute<PrimitiveValueAttribute>();

            signedMap.WriteUInt32At(valAttr.Offset, (uint)sig);
            signedMap.Seek(0, SeekOrigin.Begin);

            var header = new Span<byte>(new byte[BlamSerializer.SizeOf<H2mccMapHeader>()]);

            // Create destination map on disk, copy header
            using var outMap = File.OpenWrite(this.args.OutPath);
            signedMap.Read(header);
            outMap.Write(header);

            // Write empty compression info until we're done
            var compressionSections = new Span<byte>(new byte[8192]);
            outMap.Write(compressionSections);

            var sections = new List<H2mccCompressionSections.CompressionSection>();

            var chunk = new Span<byte>(new byte[CompressionChunkSize]);

            // Compress chunks, write to outMap
            while(signedMap.Position < signedMap.Length - 1)
            {
                var bytesToTake = Math.Min(CompressionChunkSize, signedMap.Length - signedMap.Position);
                var readBytes = signedMap.Read(chunk);

                Debug.Assert(readBytes == bytesToTake);

                using var compressed = new MemoryStream();
                using (var compressor = new DeflateStream(compressed, CompressionLevel.Optimal, true))
                {
                    compressor.Write(chunk.ToArray(), 0, readBytes);
                }

                compressed.Seek(0, SeekOrigin.Begin);

                var section = new H2mccCompressionSections.CompressionSection((uint)compressed.Length + 2, (uint)outMap.Position);
                sections.Add(section);

                // Write magic bytes
                outMap.Write(BitConverter.GetBytes((ushort)5416));

                compressed.CopyTo(outMap);
            }

            // Go back and write compression section info
            outMap.Seek(BlamSerializer.SizeOf<H2mccMapHeader>(), SeekOrigin.Begin);
            foreach(var section in sections)
            {
                outMap.WriteUInt32(section.Count);
                outMap.WriteUInt32(section.Offset);
            }
        }
    }
}
