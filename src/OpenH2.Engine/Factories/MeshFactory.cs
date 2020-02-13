using OpenH2.Core.Representations;
using OpenH2.Core.Tags;
using OpenH2.Foundation;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace OpenH2.Engine.Factories
{
    public static class MeshFactory
    {
        private static Mesh<BitmapTag>[] EmptyModel = Array.Empty<Mesh<BitmapTag>>();

        public static Mesh<BitmapTag>[] GetModelForHlmt(H2vMap map, TagRef<PhysicalModelTag> hlmtReference, out Vector3 least, out Vector3 most)
        {
            if (map.TryGetTag(hlmtReference, out var hlmt) == false)
            {
                Console.WriteLine($"Couldn't find HLMT[{hlmtReference.Id}]");
                least = Vector3.Zero;
                most = Vector3.Zero;
                return EmptyModel;
            }

            if (map.TryGetTag(hlmt.Model, out var model) == false)
            {
                Console.WriteLine($"No MODE[{hlmt.Model.Id}] found for HLMT[{hlmt.Id}]");
                least = Vector3.Zero;
                most = Vector3.Zero;
                return EmptyModel;
            }

            // TODO: determine correct BB?
            var bb = model.BoundingBoxes[0];
            least = new Vector3(bb.MinX, bb.MinY, bb.MinZ);
            most = new Vector3(bb.MaxX, bb.MaxY, bb.MaxZ);

            var renderModelMeshes = new List<Mesh<BitmapTag>>();

            foreach (var lod in model.Lods)
            {
                var partIndex = lod.Permutations[0].HighestPieceIndex;

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
