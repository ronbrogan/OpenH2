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
        public static Model<BitmapTag> UnitPyramid(Vector3 color)
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

        public static Model<BitmapTag> HalfTriangularThing(Vector3 color)
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
