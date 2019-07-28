using OpenH2.Core.Architecture;
using OpenH2.Core.Tags;
using OpenH2.Engine.Components;
using OpenH2.Foundation;
using OpenH2.Translation.TagData;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace OpenH2.Engine.Entities
{
    public class Terrain : Entity
    {
        public Terrain()
        {
            var renderComponent = new RenderModelComponent(this);

            renderComponent.Meshes = new List<Mesh>()
            {
                Cube(2f)
            };

            this.Components = new Component[]
            {
                renderComponent,
                new TransformComponent(this)
            };
        }

        public void SetComponents(Component[] components)
        {
            this.Components = components;
        }

        public Terrain(BspTagData bsp)
        {
            var comp = new RenderModelComponent(this);
            comp.Meshes = new List<Mesh>();

            foreach (var model in bsp.RenderModels)
            {
                comp.Meshes.AddRange(model.Meshes);
            }

            this.Components = new Component[]
            {
                comp,
                new TransformComponent(this)
            };
        }

        // TODO: remove, for testing purposes only
        public static Mesh Cube(float sideLength)
        {
            var cube = new Mesh();
            cube.ElementType = MeshElementType.TriangleList;

            var verts = new Vector3[9] {
                    new Vector3(),
                    new Vector3(-sideLength, -sideLength, -sideLength),
                    new Vector3(-sideLength, -sideLength, sideLength),
                    new Vector3(sideLength, -sideLength, -sideLength),
                    new Vector3(sideLength, -sideLength, sideLength),
                    new Vector3(sideLength, sideLength, sideLength),
                    new Vector3(-sideLength, sideLength, sideLength),
                    new Vector3(sideLength, sideLength, -sideLength),
                    new Vector3(-sideLength, sideLength, -sideLength),
            };

            var norms = new Vector3[7] {
                    new Vector3(),
                    new Vector3(0f, -1.0f, 0f),
                    new Vector3(0f, 1.0f, 0f),
                    new Vector3(0f, 0f, 1.0f),
                    new Vector3(1.0f, 0f, 0f),
                    new Vector3(0f, 0f, -1.0f),
                    new Vector3(-1.0f, 0f, 0f),
            };

            var tex = new Vector2[15] {
                    new Vector2(),
                    new Vector2(0.5f, 1f),
                    new Vector2(0.25f, 1f),
                    new Vector2(0.5f, 0.666666f),
                    new Vector2(0.25f, 0.666666f),
                    new Vector2(0.5f, 0.333333f),
                    new Vector2(0.5f, 0f),
                    new Vector2(0.25f, 0.333333f),
                    new Vector2(0.25f, 0f),
                    new Vector2(0.75f, 0.666666f),
                    new Vector2(0.75f, 0.333333f),
                    new Vector2(0f, 0.666666f),
                    new Vector2(0f, 0.333333f),
                    new Vector2(1f, 0.666666f),
                    new Vector2(1f, 0.333333f),
            };

            cube.Verticies = new VertexFormat[]
            {
                // 1
                new VertexFormat(verts[1], tex[1], norms[1]),
                new VertexFormat(verts[2], tex[2], norms[1]),
                new VertexFormat(verts[3], tex[3], norms[1]),

                new VertexFormat(verts[4], tex[4], norms[1]),
                new VertexFormat(verts[3], tex[3], norms[1]),
                new VertexFormat(verts[2], tex[2], norms[1]),

                // 3
                new VertexFormat(verts[5], tex[5], norms[2]),
                new VertexFormat(verts[6], tex[6], norms[2]),
                new VertexFormat(verts[7], tex[7], norms[2]),

                new VertexFormat(verts[8], tex[8], norms[2]),
                new VertexFormat(verts[7], tex[7], norms[2]),
                new VertexFormat(verts[6], tex[6], norms[2]),

                // 5
                new VertexFormat(verts[4], tex[3], norms[3]),
                new VertexFormat(verts[2], tex[9], norms[3]),
                new VertexFormat(verts[5], tex[5], norms[3]),

                new VertexFormat(verts[6], tex[10], norms[3]),
                new VertexFormat(verts[5], tex[5], norms[3]),
                new VertexFormat(verts[2], tex[9], norms[3]),

                // 7
                new VertexFormat(verts[3], tex[4], norms[4]),
                new VertexFormat(verts[4], tex[3], norms[4]),
                new VertexFormat(verts[7], tex[7], norms[4]),

                new VertexFormat(verts[5], tex[5], norms[4]),
                new VertexFormat(verts[7], tex[7], norms[4]),
                new VertexFormat(verts[4], tex[3], norms[4]),
                
                // 9
                new VertexFormat(verts[1], tex[11], norms[5]),
                new VertexFormat(verts[3], tex[4], norms[5]),
                new VertexFormat(verts[8], tex[12], norms[5]),

                new VertexFormat(verts[7], tex[7], norms[5]),
                new VertexFormat(verts[8], tex[12], norms[5]),
                new VertexFormat(verts[3], tex[4], norms[5]),

                // 11
                new VertexFormat(verts[2], tex[9], norms[6]),
                new VertexFormat(verts[1], tex[13], norms[6]),
                new VertexFormat(verts[6], tex[10], norms[6]),

                new VertexFormat(verts[8], tex[14], norms[6]),
                new VertexFormat(verts[6], tex[10], norms[6]),
                new VertexFormat(verts[1], tex[13], norms[6]),
            };

            cube.Indicies = new int[]
            {
                0,1,2,3,4,5,6,7,8,9,10,
                11,12,13,14,15,16,17,18,
                19,20,21,22,23,24,25,26,27,28,
                29,30,31,32,33,34,35
            };

            cube.MaterialIdentifier = 1;

            return cube;
        }
    }
}
