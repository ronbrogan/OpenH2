using OpenH2.Core.Tags;
using OpenH2.Foundation;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace OpenH2.Core.Factories
{
    public static class ModelFactory
    {
        public static Model<BitmapTag> Cuboid(Vector3 lower, Vector3 upper, Vector4 color)
        {
            var mesh = new Mesh<BitmapTag>
            {
                ElementType = MeshElementType.TriangleList,
                Indicies = new[] {
                    0, 2, 3,
                    0, 3, 1,
                    0, 5, 6,
                    0, 1, 5,
                    0, 4, 2,
                    0, 6, 4, 
                    7, 5, 1, 
                    7, 1, 3, 
                    7, 2, 4, 
                    7, 3, 2,
                    7, 4, 6,
                    7, 6, 5
                },
                Verticies = new VertexFormat[] {
                    new VertexFormat(lower, new Vector2(), new Vector3()),
                    new VertexFormat(new Vector3(upper.X, lower.Y, lower.Z), new Vector2(), new Vector3()),
                    new VertexFormat(new Vector3(lower.X, upper.Y, lower.Z), new Vector2(), new Vector3()),
                    new VertexFormat(new Vector3(upper.X, upper.Y, lower.Z), new Vector2(), new Vector3()),
                    new VertexFormat(new Vector3(lower.X, upper.Y, upper.Z), new Vector2(), new Vector3()),
                    new VertexFormat(new Vector3(upper.X, lower.Y, upper.Z), new Vector2(), new Vector3()),
                    new VertexFormat(new Vector3(lower.X, lower.Y, upper.Z), new Vector2(), new Vector3()),
                    new VertexFormat(upper, new Vector2(), new Vector3())
                },
                Material = new Material<BitmapTag>() { DiffuseColor = color }
            };

            return new Model<BitmapTag>()
            {
                Meshes = new[] { mesh },
                Flags = ModelFlags.Diffuse
            };
        }

        public static Model<BitmapTag> UnitPyramid(Vector4 color)
        {
            var mesh = new Mesh<BitmapTag>
            {
                ElementType = MeshElementType.TriangleList,
                Indicies = new[] {
                    0, 2, 1,
                    0, 1, 3,
                    1, 2, 3,
                    2, 0, 3 },
                Verticies = new VertexFormat[] {
                    new VertexFormat(new Vector3(0, 0, 0), new Vector2(), new Vector3()),
                    new VertexFormat(new Vector3(0.5f, 0, 0), new Vector2(), new Vector3()),
                    new VertexFormat(new Vector3(0.25f, 0.5f, 0), new Vector2(), new Vector3()),
                    new VertexFormat(new Vector3(0.25f, 0.25f, 0.5f), new Vector2(), new Vector3())
                },
                Material = new Material<BitmapTag>() { DiffuseColor = color }
            };

            return new Model<BitmapTag>()
            {
                Meshes = new[] { mesh },
                Flags = ModelFlags.Diffuse
            };
        }

        public static Model<BitmapTag> HalfTriangularThing(Vector4 color)
        {
            var mesh = new Mesh<BitmapTag>
            {
                ElementType = MeshElementType.TriangleList,
                Indicies = new[] {
                    1, 0, 4,
                    2, 1, 4,
                    0, 2, 4,
                    0, 1, 3,
                    1, 2, 3,
                    2, 0, 3 },
                Verticies = new VertexFormat[] {
                    new VertexFormat(new Vector3(0, 0, 0), new Vector2(), new Vector3()),
                    new VertexFormat(new Vector3(0.25f, 0, 0), new Vector2(), new Vector3()),
                    new VertexFormat(new Vector3(0.12f, 0.25f, 0), new Vector2(), new Vector3()),
                    new VertexFormat(new Vector3(0.12f, 0.12f, 0.25f), new Vector2(), new Vector3()),
                    new VertexFormat(new Vector3(0.12f, 0.12f, -0.25f), new Vector2(), new Vector3())
                },
                Material = new Material<BitmapTag>() { DiffuseColor = color }
            };

            return new Model<BitmapTag>()
            {
                Meshes = new[] { mesh },
                Flags = ModelFlags.Diffuse
            };
        }
    }
}
