using System;
using System.Collections.Generic;
using OpenH2.Core.Extensions;
using System.IO;
using OpenH2.Core.Tags.Common;

namespace OpenH2.BspMetaAnalysis
{
    public class BlkhScanner
    {
        private static uint BLKH = 1651272552;
        private static uint RSRC = 1920168547;

        public static void ScanBlocks(FileStream file)
        {
            var data = file.ToMemory().Span;

            var blocksLocations = new List<int>();

            for (var i = 0; i < data.Length; i += 4)
            {
                if(data.ReadUInt32At(i) == 1651272552)
                {
                    blocksLocations.Add(i);
                }
            }

            foreach(var blockLoc in blocksLocations)
            {
                int offset = blockLoc;
                uint val = 0;
                while(val != RSRC)
                {
                    offset += 4;
                    val = data.ReadUInt32At(offset);
                }

                var bodyLength = data.ReadInt32At(blockLoc + 4);

                var blockData = data.Slice(blockLoc, offset - blockLoc + bodyLength);

                ProcessBlock(blockLoc, blockData, offset - blockLoc - 4);
            }
        }


        private static void ProcessBlock(int start, Span<byte> blockData, int headerLength)
        {
            var sections = new List<(int, int)>();

            var currentSectionStart = headerLength + 8;

            for(var i = currentSectionStart; i < blockData.Length; i+=4)
            {
                if(blockData.ReadUInt32At(i) == RSRC)
                {
                    sections.Add((currentSectionStart, i - currentSectionStart));

                    currentSectionStart = i + 4;
                }
            }

            

            Console.WriteLine($"BLKH@{start} - Sections:{sections.Count} - HeaderSize:{headerLength}");
            foreach ((var s, var l) in sections)
                Console.WriteLine($"\tStart:{s}\tLength:{l}");

            if(headerLength < 68)
            {
                return;
            }

            var headerData = blockData.Slice(0, headerLength);

            var header = new ModelResourceBlockHeader()
            {
                PartInfoCount = headerData.ReadUInt32At(8),
                PartInfo2Count = headerData.ReadUInt32At(16),
                PartInfo3Count = headerData.ReadUInt32At(24),
                IndexCount = headerData.ReadUInt32At(40),
                UknownDataLength = headerData.ReadUInt32At(48),
                UknownIndiciesCount = headerData.ReadUInt32At(56),
                VertexComponentCount = headerData.ReadUInt32At(64)
            };


        }
    }
}
