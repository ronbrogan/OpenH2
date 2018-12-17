using System;
using System.Collections.Generic;
using System.Text;
using OpenH2.Core.Extensions;
using OpenH2.Core.Parsing;
using OpenH2.Core.Representations;

namespace OpenH2.Core.Meta.Processors
{
    public static class ScenarioMetaProcessor
    {
        public static ScenarioMeta ProcessScenario(string name, ObjectIndexEntry index, TrackingChunk chunk)
        {
            var data = chunk.Span;
            var meta = new ScenarioMeta
            {
                Name = name,
                RawMeta = data.ToArray()
            };

            meta.SkyboxIds = GetSkyboxIds(data, index);
            meta.Terrains = GetTerrains(data, index);

            return meta;
        }

        private static uint[] GetSkyboxIds(Span<byte> data, ObjectIndexEntry index)
        {
            var skyboxCao = data.ReadMetaCaoAt(8, index);
            var skyboxSectionLength = 8;

            var skyboxes = new uint[skyboxCao.Count];

            for(var i = 0; i < skyboxCao.Count; i++)
            {
                var span = data.Slice(skyboxCao.Offset.Value + i * skyboxSectionLength, skyboxSectionLength);
                skyboxes[i] = span.ReadUInt32At(4);
            }

            return skyboxes;
        }

        private static ScenarioMeta.Terrain[] GetTerrains(Span<byte> data, ObjectIndexEntry index)
        {
            var cao = data.ReadMetaCaoAt(528, index);
            var terrainLength = 68;

            var terrains = new ScenarioMeta.Terrain[cao.Count];

            for (var i = 0; i < cao.Count; i++)
            {
                var span = data.Slice(cao.Offset.Value + i * terrainLength, terrainLength);

                var terrain = new ScenarioMeta.Terrain()
                {
                    BspId = span.ReadUInt32At(20),
                    LightmapId = span.ReadUInt32At(28),
                };

                terrains[i] = terrain;
            }

            return terrains;
        }

    }
}
