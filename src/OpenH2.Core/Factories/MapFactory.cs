﻿using OpenBlam.Core.Extensions;
using OpenBlam.Core.MapLoading;
using OpenBlam.Core.Maps;
using OpenBlam.Core.Streams;
using OpenBlam.Serialization;
using OpenH2.Core.Enums;
using OpenH2.Core.Extensions;
using OpenH2.Core.Maps;
using OpenH2.Core.Maps.MCC;
using OpenH2.Core.Maps.Vista;
using OpenH2.Core.Offsets;
using OpenH2.Core.Tags;
using System;
using System.Collections.Generic;
using System.IO;

namespace OpenH2.Core.Factories
{
    public class MapFactory
    {
        private const string MainMenuName = "mainmenu.map";
        private const string MultiPlayerSharedName = "shared.map";
        private const string SinglePlayerSharedName = "single_player_shared.map";
        private readonly string mapRoot;
        private MapLoader loader;

        public MapFactory(string mapRoot)
        {
            this.loader = MapLoaderBuilder.FromRoot(mapRoot)
                .UseAncillaryMap((byte)DataFile.MainMenu, MainMenuName)
                .UseAncillaryMap((byte)DataFile.SinglePlayerShared, SinglePlayerSharedName)
                .UseAncillaryMap((byte)DataFile.Shared, MultiPlayerSharedName)
                .Build();
            this.mapRoot = mapRoot;
        }

        public IH2Map Load(string mapFileName)
        {
            Span<byte> header = new byte[2048];
            using (var peek = File.OpenRead(Path.Combine(this.mapRoot, mapFileName)))
            {
                peek.Read(header);
            }

            var baseHeader = BlamSerializer.Deserialize<H2HeaderBase>(header);

            return baseHeader.Version switch
            {
                MapVersion.Halo2 => LoadH2Map(mapFileName, header),
                MapVersion.Halo2Mcc => LoadH2mccMap(mapFileName),
                MapVersion.Halo2MccSeason8 => LoadH2mccMapV13(mapFileName),
                _ => throw new NotSupportedException()
            };
        }

        public static IH2MapInfo LoadInformational(string mapPath)
        {
            // Not using ancillary maps here, just info from map header
            var singleLoader = MapLoader.FromRoot(Path.GetDirectoryName(mapPath));

            Span<byte> header = new byte[2048];
            using (var peek = File.OpenRead(mapPath))
            {
                peek.Read(header);
            }

            var baseHeader = BlamSerializer.Deserialize<H2HeaderBase>(header);

            return baseHeader.Version switch
            {
                MapVersion.Halo2 => singleLoader.Load<H2vMapInfo>(
                    new ReadOnlyFileStream(mapPath), 
                    (IMap map, Stream stream) => {}),

                _ => throw new NotSupportedException("This map type is not supported")
            };
        }

        public IH2Map LoadH2Map(string mapFileName, Span<byte> headerData)
        {
            // Vista and Xbox use the same version, using header layout to differentiate
            // If the stored signature according to the Vista layout is 0, it's an Xbox map
            if(headerData.ReadUInt32At(BlamSerializer.StartsAt<H2vMapHeader>(h => h.StoredSignature)) == 0)
            {
                throw new NotSupportedException("Xbox maps aren't supported yet");
            }

            return this.loader.Load<H2vMap>(mapFileName, LoadMetadata);
        }

        public H2mccMap LoadH2mccMap(string mapFileName)
        {
            return this.loader.Load<H2mccMap>(mapFileName, H2mccCompression.DecompressInline, LoadMetadata);
        }

        public H2mccV13Map LoadH2mccMapV13(string mapFileName)
        {
            return this.loader.Load<H2mccV13Map>(mapFileName, H2mccCompressionV13.DecompressInline, LoadMetadata);
        }

        public H2mccMap LoadSingleH2mccMap(Stream decompressedMap)
        {
            // Not using ancillary maps here since the map stream is already 
            // decompressed, while the ancillary maps are still compressed
            var singleLoader = MapLoader.FromRoot(this.mapRoot);

            return singleLoader.Load<H2mccMap>(decompressedMap, LoadMetadata);
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
    }
}
