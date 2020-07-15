using OpenH2.Core.Representations;
using OpenH2.Core.Tags;
using OpenH2.Foundation;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace OpenH2.Engine.Factories
{
    public static class MeshFactory
    {
        private static Mesh<BitmapTag>[] EmptyModel = Array.Empty<Mesh<BitmapTag>>();

        private static ConcurrentDictionary<ulong, Mesh<BitmapTag>[]> meshes = new ConcurrentDictionary<ulong, Mesh<BitmapTag>[]>();

        public static Mesh<BitmapTag>[] GetRenderModel(H2vMap map, TagRef<HaloModelTag> hlmtReference, int damageLevel = 0)
        {
            var key = (((ulong)hlmtReference.Id) << 32) | (ulong)damageLevel;

            return meshes.GetOrAdd(key, _ => Create(map, hlmtReference, damageLevel));
        }

        private static Mesh<BitmapTag>[] Create(H2vMap map, TagRef<HaloModelTag> hlmtReference, int damageLevel)
        {
            if (map.TryGetTag(hlmtReference, out var hlmt) == false)
            {
                Console.WriteLine($"Couldn't find HLMT[{hlmtReference.Id}]");
                return EmptyModel;
            }

            if (map.TryGetTag(hlmt.RenderModel, out var model) == false)
            {
                Console.WriteLine($"No MODE[{hlmt.RenderModel.Id}] found for HLMT[{hlmt.Id}]");
                return EmptyModel;
            }

            var renderModelMeshes = new List<Mesh<BitmapTag>>();

            foreach (var lod in model.Components)
            {
                var partIndex = lod.DamageLevels[damageLevel].HighestPieceIndex;

                foreach (var mesh in model.Parts[partIndex].Model.Meshes)
                {
                    var mat = map.CreateMaterial(mesh);

                    renderModelMeshes.Add(new Mesh<BitmapTag>()
                    {
                        Compressed = mesh.Compressed,
                        ElementType = mesh.ElementType,
                        Indicies = mesh.Indices,
                        Note = mesh.Note,
                        RawData = mesh.RawData,
                        Verticies = mesh.Verticies,

                        Material = mat
                    });
                }
            }

            return renderModelMeshes.ToArray();
        }
    }
}
