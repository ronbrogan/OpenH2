using OpenH2.Core.Maps;
using OpenH2.Core.Maps.Vista;
using OpenH2.Core.Tags;
using OpenH2.Foundation;
using OpenH2.Physics.Colliders;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace OpenH2.Engine.Factories
{
    public static class MeshFactory
    {
        private static Mesh<BitmapTag>[] EmptyModel = Array.Empty<Mesh<BitmapTag>>();

        private static ConcurrentDictionary<ulong, Mesh<BitmapTag>[]> meshes = new();
        private static ConcurrentDictionary<object, Mesh<BitmapTag>[]> colliderModels = new();

        private static Material<BitmapTag> BoneMaterial = new Material<BitmapTag>() { DiffuseColor = new Vector4(1f, 0, 0, 1f) };

        public static Mesh<BitmapTag>[] GetRenderModel(H2vMap map, TagRef<HaloModelTag> hlmtReference, int damageLevel = 0)
        {
            var key = (((ulong)hlmtReference.Id) << 32) | (ulong)damageLevel;

            return meshes.GetOrAdd(key, _ => Create(map, hlmtReference, damageLevel));
        }

        public static Mesh<BitmapTag>[] GetRenderModel(TriangleMeshCollider collider, Vector4 color = default)
        {
            if(color == default)
            {
                color = new Vector4(0f, 1f, 0f, 1f);
            }

            return colliderModels.GetOrAdd(collider, _ => new Mesh<BitmapTag>[] { Create(collider, color) });
        }

        public static Mesh<BitmapTag>[] GetRenderModel(TriangleModelCollider collider, Vector4 color = default)
        {
            if (color == default)
            {
                color = new Vector4(0f, 1f, 0f, 1f);
            }

            return colliderModels.GetOrAdd(collider, _ => collider.MeshColliders.Select(m => Create(m, color)).ToArray());
        }

        public static Mesh<BitmapTag>[] GetRenderModel(ConvexMeshCollider collider, Vector4 color = default)
        {
            if (color == default)
            {
                color = new Vector4(0f, 1f, 0f, 1f);
            }

            return colliderModels.GetOrAdd(collider, _ => new Mesh<BitmapTag>[] { Create(collider.Vertices, color) });
        }

        public static Mesh<BitmapTag>[] GetRenderModel(ConvexModelCollider collider, Vector4 color = default)
        {
            if (color == default)
            {
                color = new Vector4(0f, 1f, 0f, 1f);
            }

            return colliderModels.GetOrAdd(collider, _ => collider.Meshes.Select(m => Create(m, color)).ToArray());
        }

        public static Mesh<BitmapTag>[] GetBonesModel(H2vMap map, TagRef<HaloModelTag> hlmtReference, int damageLevel = 0)
        {
            return CreateBoneMeshes(map, hlmtReference, damageLevel);
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

        private static Mesh<BitmapTag> Create(TriangleMeshCollider collider, Vector4 color)
        {
            var verts = new VertexFormat[collider.Vertices.Length];

            for (int i = 0; i < collider.Vertices.Length; i++)
            {
                verts[i] = new VertexFormat(collider.Vertices[i], new Vector2(), new Vector3());
            }

            var mesh = new Mesh<BitmapTag>()
            {
                ElementType = MeshElementType.TriangleList,
                Indicies = collider.TriangleIndices,
                Note = "TriangleMeshCollider",
                Verticies = verts,
                Material = new Material<BitmapTag>() { DiffuseColor = color }
            };

            return mesh;
        }

        private static Mesh<BitmapTag> Create(Vector3[] convexPoints, Vector4 color)
        {
            var verts = new VertexFormat[convexPoints.Length];

            for (int i = 0; i < convexPoints.Length; i++)
            {
                verts[i] = new VertexFormat(convexPoints[i], new Vector2(), new Vector3());
            }

            var triCount = convexPoints.Length - 2;
            var indices = new int[triCount];

            for(var i = 0; i < triCount-2; i++)
            {
                indices[i] = i;
                indices[i+1] = i+1;
                indices[i+2] = i+2;
            }

            var mesh = new Mesh<BitmapTag>()
            {
                ElementType = MeshElementType.TriangleList,
                Indicies = indices,
                Note = "ConvexMeshCollider",
                Verticies = verts,
                Material = new Material<BitmapTag>() { DiffuseColor = color }
            };

            return mesh;
        }

        private static Mesh<BitmapTag>[] CreateBoneMeshes(H2vMap map, TagRef<HaloModelTag> hlmtReference, int damageLevel)
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

            var transforms = new Matrix4x4[model.Bones.Length];

            for (int i = 0; i < model.Bones.Length; i++)
            {
                RenderModelTag.Bone bone = model.Bones[i];
                var translate = Matrix4x4.CreateTranslation(bone.Translation);
                var scale = Matrix4x4.CreateScale(1);

                Matrix4x4 rotate = Matrix4x4.CreateFromQuaternion(bone.Orientation);

                var scaleRotate = Matrix4x4.Multiply(scale, rotate);

                var result = Matrix4x4.Multiply(scaleRotate, translate);
                transforms[i] = result;
            }

            for (int i = 0; i < model.Bones.Length; i++)
            {
                var bone = model.Bones[i];
                var hierarchyTransforms = new Stack<Matrix4x4>();
                hierarchyTransforms.Push(transforms[i]);

                var parentIndex = bone.ParentIndex;
                while (parentIndex != -1)
                {
                    hierarchyTransforms.Push(transforms[parentIndex]);
                    parentIndex = model.Bones[parentIndex].ParentIndex;
                }

                var finalTransform = Matrix4x4.Identity;

                while (hierarchyTransforms.TryPop(out var mat))
                    finalTransform = Matrix4x4.Multiply(mat, finalTransform);

                var boneLength = 0.01f;

                if(bone.FirstChildIndex != -1)
                {
                    boneLength = model.Bones[bone.FirstChildIndex].Translation.Length();
                }

                var end = Vector3.Transform(new Vector3(boneLength, 0, 0), finalTransform);
                var base1 = Vector3.Transform(new Vector3(0, 0f, 0.01f), finalTransform);
                var base2 = Vector3.Transform(new Vector3(0, -0.01f, -0.01f), finalTransform);
                var base3 = Vector3.Transform(new Vector3(0, 0.01f, -0.01f), finalTransform);

                var boneMesh = new Mesh<BitmapTag>
                {
                    ElementType = MeshElementType.TriangleList,
                    Indicies = new[] {
                        0, 2, 1,
                        0, 1, 3,
                        1, 2, 3,
                        2, 0, 3 },
                    Verticies = new VertexFormat[] {
                        new VertexFormat(base1, new Vector2(), new Vector3()),
                        new VertexFormat(base2, new Vector2(), new Vector3()),
                        new VertexFormat(base3, new Vector2(), new Vector3()),
                        new VertexFormat(end, new Vector2(), new Vector3())
                    },
                    Material = BoneMaterial,
                };

                renderModelMeshes.Add(boneMesh);
            }

            return renderModelMeshes.ToArray();
        }
    }
}