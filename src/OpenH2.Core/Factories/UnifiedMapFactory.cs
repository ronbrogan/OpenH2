using OpenBlam.Core.MapLoading;
using OpenBlam.Core.Maps;
using OpenBlam.Core.Streams;
using OpenBlam.Serialization;
using OpenH2.Core.Enums;
using OpenH2.Core.Extensions;
using OpenH2.Core.Maps;
using OpenH2.Core.Maps.MCC;
using OpenH2.Core.Offsets;
using OpenH2.Core.Tags;
using System.Collections.Generic;
using System.IO;

namespace OpenH2.Core.Factories
{
    public class UnifiedMapFactory
    {
        private const string MainMenuName = "mainmenu.map";
        private const string MultiPlayerSharedName = "shared.map";
        private const string SinglePlayerSharedName = "single_player_shared.map";
        private MapLoader loader;

        public UnifiedMapFactory(string mapRoot)
        {
            this.loader = MapLoaderBuilder.FromRoot(mapRoot)
                .UseAncillaryMap((byte)DataFile.MainMenu, MainMenuName)
                .UseAncillaryMap((byte)DataFile.SinglePlayerShared, SinglePlayerSharedName)
                .UseAncillaryMap((byte)DataFile.Shared, MultiPlayerSharedName)
                .Build();
        }

        public IH2Map Load(string mapFileName)
        {
            // TODO: implement and dispatch mcc loading
            return LoadH2vMap(mapFileName);
        }

        public H2vMap LoadH2vMap(string mapFileName)
        {
            return this.loader.Load<H2vMap>(mapFileName, LoadMetadata);
        }

        private static void LoadMetadata(IMap map, Stream reader)
        {
            if(map is not IH2Map h2map)
            {
                return;
            }

            h2map.Header.SecondaryOffset = h2map.PrimaryOffset(h2map.Header.RawSecondaryOffset);
            h2map.IndexHeader = DeserializeIndexHeader(h2map, reader);
            h2map.PrimaryMagic = CalculatePrimaryMagic(h2map.IndexHeader);
            h2map.TagIndex = BuildTagIndex(h2map, reader, out var firstOffset);
            h2map.SecondaryMagic = CalculateSecondaryMagic(h2map.Header, firstOffset);
            h2map.LoadWellKnownTags();
        }

        public static IndexHeader DeserializeIndexHeader(IH2Map scene, Stream reader)
        {
            var header = scene.Header;

            var index = BlamSerializer.Deserialize<IndexHeader>(reader, header.IndexOffset.Value);
            index.FileRawOffset = header.IndexOffset;
            index.TagIndexOffset = scene.PrimaryOffset(index.RawTagIndexOffset);

            return index;
        }

        public static Dictionary<uint, TagIndexEntry> BuildTagIndex(IH2Map scene, Stream reader, out int firstEntryOffset)
        {
            firstEntryOffset = -1;
            var index = scene.IndexHeader;

            var entries = new Dictionary<uint, TagIndexEntry>(index.TagIndexCount);

            for (var i = 0; i < index.TagIndexCount; i++)
            {
                var entryBase = index.TagIndexOffset.Value + i * 16;

                var tag = (TagName)reader.ReadUInt32At(entryBase);

                if (tag == TagName.NULL)
                    continue;

                var entry = new TagIndexEntry
                {
                    Tag = tag,
                    ID = reader.ReadUInt32At(entryBase + 4),
                    Offset = new SecondaryOffset(scene, reader.ReadInt32At(entryBase + 8)),
                    DataSize = reader.ReadInt32At(entryBase + 12)
                };

                if (entry.DataSize == 0)
                    continue;

                if (firstEntryOffset == -1)
                    firstEntryOffset = entry.Offset.OriginalValue;

                entries[entry.ID] = entry;
            }

            return entries;
        }

        public static int CalculatePrimaryMagic(IndexHeader index)
        {
            return index.FileRawOffset.Value - index.PrimaryMagicConstant + IndexHeader.Length;
        }

        public static int CalculateSecondaryMagic(IH2MapHeader header, int firstObjOffset)
        {
            return firstObjOffset - header.SecondaryOffset.Value;
        }

        private void AssertMccData(Stream reader)
        {
            var fourCC = reader.ReadUInt32At(0);
            if (fourCC != H2mccCompression.DecompressedFourCC)
            {
                throw new System.Exception("Cannot load a map that hasn't been decompressed first");
            }
        }
    }
}
