using System;
using System.Collections.Generic;
using System.Text;
using OpenH2.Core.Extensions;
using OpenH2.Core.Parsing;
using OpenH2.Core.Representations;

namespace OpenH2.Core.Tags.Processors
{
    public static class ScenarioTagProcessor
    {
        public static Scenario ProcessScenario(uint id, string name, TagIndexEntry index, TrackingChunk chunk, TrackingReader sceneReader)
        {
            var data = chunk.Span;
            var meta = new Scenario(id)
            {
                Name = name,
                RawMeta = data.ToArray()
            };

            meta.SkyboxIds = GetSkyboxIds(data, index);
            meta.Terrains = GetTerrains(data, index);

            return meta;
        }

        private static uint[] GetSkyboxIds(Span<byte> data, TagIndexEntry index)
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

        private static Scenario.Terrain[] GetTerrains(Span<byte> data, TagIndexEntry index)
        {
            var cao = data.ReadMetaCaoAt(528, index);
            var terrainLength = 68;

            var terrains = new Scenario.Terrain[cao.Count];

            for (var i = 0; i < cao.Count; i++)
            {
                var span = data.Slice(cao.Offset.Value + i * terrainLength, terrainLength);

                var terrain = new Scenario.Terrain()
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
