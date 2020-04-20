using OpenH2.Core.Factories;
using System.IO;
using System.Linq;
using System.Text;
using System.Drawing;
using System;
using System.Collections.Generic;
using OpenH2.Core.Tags;
using OpenH2.Core.Representations;
using System.Numerics;

namespace OpenH2.BspMetaAnalysis
{
    class Program
    {
        static void Main(string[] args)
        {
            var mapName = @"D:\H2vMaps\containment.map";

            H2vMap map;

            var fac = new MapFactory(Path.GetDirectoryName(mapName), new MaterialFactory(Environment.CurrentDirectory + "\\Configs"));
            using (var fs = new FileStream(mapName, FileMode.Open))
                map = fac.FromFile(fs);

            var bsps = map.GetLocalTagsOfType<BspTag>().ToArray();

            for(var i = 0; i < bsps.Count(); i++)
            {
                var bsp = bsps[i];

                var obj = CreateCollisionObj(bsp);
                File.WriteAllText($"D:\\bsp_{i}_collision.obj", obj);
            }
        }

        public static string CreateCollisionObj(BspTag tag)
        {
            var sb = new StringBuilder();
            var vertsAdded = 0;

            Console.WriteLine($"CollisionInfo lengths: {tag.CollisionInfos.Length}");

            sb.AppendLine("o CollisionMesh");

            foreach (var col in tag.CollisionInfos)
            {
                var nextFaceSlot = 0;
                var faceTexLookup = new Dictionary<int, int>();

                foreach(var face in col.Faces)
                {
                    if(faceTexLookup.ContainsKey(face.ShaderIndex))
                    {
                        continue;
                    }

                    faceTexLookup.Add(face.ShaderIndex, nextFaceSlot + 1);
                    nextFaceSlot++;
                }

                for(var i = 0; i < nextFaceSlot; i++)
                {
                    sb.AppendLine($"vt 0.5 {(i / (float)nextFaceSlot).ToString("0.000000")}");
                }


                var faceIndex = 0;
                for(; faceIndex < col.Faces.Length; faceIndex++)
                {
                    var face = col.Faces[faceIndex];
                    var verts = new List<ushort>(8);

                    ushort edgeIndex = face.FirstEdge; 
                    do
                    {
                        var edge = col.HalfEdges[edgeIndex];

                        verts.Add(edge.Face0 == faceIndex
                            ? edge.Vertex0
                            : edge.Vertex1);

                        edgeIndex = edge.Face0 == faceIndex 
                            ? edge.NextEdge
                            : edge.PrevEdge;

                    } while (edgeIndex != face.FirstEdge);

                    var faceTexCoord = faceTexLookup[face.ShaderIndex];

                    foreach (var index in verts)
                    {
                        var vert = col.Verticies[index];
                        sb.AppendLine($"v {vert.x.ToString("0.000000")} {vert.y.ToString("0.000000")} {vert.z.ToString("0.000000")}");
                    }

                    sb.Append($"f");

                    foreach(var vert in verts)
                    {
                        sb.Append(" ");
                        sb.Append($"{++vertsAdded}/{faceTexCoord}");
                    }

                    sb.AppendLine();
                }
            }


            return sb.ToString();
        }

        public static string CreatePlanesObj(BspTag tag)
        {
            var sb = new StringBuilder();
            var vertsAdded = 0;

            Console.WriteLine($"CollisionInfo lengths: {tag.CollisionInfos.Length}");

            sb.AppendLine("o CollisionPlanes");

            foreach(var col in tag.CollisionInfos)
            {
                foreach(var plane in col.Planes)
                {
                    var centroid = plane.Normal * plane.Distance;

                    // Use arbitrary vector to get tangent vector to normal
                    var tempVec = Vector3.Normalize(new Vector3(plane.Normal.X + 1, plane.Normal.Y, plane.Normal.Z));
                    var tangent = Vector3.Normalize(Vector3.Cross(plane.Normal, tempVec));
                    var bitangent = Vector3.Cross(plane.Normal, tangent);

                    var upperRight = centroid + tangent + bitangent;
                    var lowerRight = centroid - tangent + bitangent;
                    var upperLeft = centroid + tangent - bitangent;
                    var lowerLeft = centroid - tangent - bitangent;

                    sb.AppendLine($"v {upperRight.X.ToString("0.000000")} {upperRight.Y.ToString("0.000000")} {upperRight.Z.ToString("0.000000")}");
                    sb.AppendLine($"v {lowerRight.X.ToString("0.000000")} {lowerRight.Y.ToString("0.000000")} {lowerRight.Z.ToString("0.000000")}");
                    sb.AppendLine($"v {lowerLeft.X.ToString("0.000000")} {lowerLeft.Y.ToString("0.000000")} {lowerLeft.Z.ToString("0.000000")}");
                    sb.AppendLine($"v {upperLeft.X.ToString("0.000000")} {upperLeft.Y.ToString("0.000000")} {upperLeft.Z.ToString("0.000000")}");

                    sb.AppendLine($"f {++vertsAdded} {++vertsAdded} {++vertsAdded} {++vertsAdded}");
                }
            }


            return sb.ToString();
        }

        public static string CreateMtlFileForBsp(BspTag tag)
        {
            var sb = new StringBuilder();

            var alreadyGenerated = new HashSet<uint>();

            for (var i = 0; i < tag.RenderChunks.Length; i++)
            {
                var model = tag.RenderChunks[i].Model;

                foreach (var mesh in model.Meshes)
                {
                    var matId = mesh.Shader.Id + 1;

                    if (alreadyGenerated.Contains(matId))
                        continue;

                    var color = GenerateRandomColor();

                    sb.AppendLine($"newmtl {matId}");
                    sb.AppendLine($"Kd {(color.R / 255f).ToString("0.000000")} {(color.G / 255f).ToString("0.000000")} {(color.B / 255f).ToString("0.000000")}");
                    sb.AppendLine($"Ka {(color.R / 255f).ToString("0.000000")} {(color.G / 255f).ToString("0.000000")} {(color.B / 255f).ToString("0.000000")}");
                    sb.AppendLine("Ks 1.000 1.000 1.000");
                    sb.AppendLine("Ns 10.000");
                    sb.AppendLine("");

                    alreadyGenerated.Add(matId);
                }
            }


            return sb.ToString();
        }

        public static Color GenerateRandomColor()
        {
            var mix = Color.Gray;

            Random random = new Random();
            int red = random.Next(256);
            int green = random.Next(256);
            int blue = random.Next(256);

            // mix the color
            if (mix != null)
            {
                red = (red + mix.R) / 2;
                green = (green + mix.G) / 2;
                blue = (blue + mix.B) / 2;
            }

            Color color = Color.FromArgb(255, red, green, blue);
            return color;
        }

        public static string CreatObjFileForBsp(BspTag tag)
        {
            var sb = new StringBuilder();

            var vertsWritten = 1;

            for(var i = 0; i < tag.RenderChunks.Length; i++)
            {
                var model = tag.RenderChunks[i].Model;
                sb.AppendLine($"o BspChunk.{i}");

                var verts = model.Meshes.First().Verticies;

                foreach (var vert in verts)
                {
                    sb.AppendLine($"v {vert.Position.X.ToString("0.000000")} {vert.Position.Y.ToString("0.000000")} {vert.Position.Z.ToString("0.000000")}");
                }

                foreach (var vert in verts)
                {
                    sb.AppendLine($"vt {vert.TexCoords.X.ToString("0.000000")} {vert.TexCoords.Y.ToString("0.000000")}");
                }

                foreach (var vert in verts)
                {
                    sb.AppendLine($"vn {vert.Normal.X.ToString("0.000000")} {vert.Normal.Y.ToString("0.000000")} {vert.Normal.Z.ToString("0.000000")}");
                }


                foreach (var mesh in model.Meshes)
                {
                    var matId = mesh.Shader.Id+1;

                    sb.AppendLine($"g BspChunk.{i}.{matId}");
                    sb.AppendLine($"usemtl {matId}");
                    
                    for(var j = 0; j < mesh.Indices.Length; j+=3)
                    {
                        var indicies = (mesh.Indices[j], mesh.Indices[j+1], mesh.Indices[j+2]);

                        sb.Append("f");
                        sb.Append($" {indicies.Item1 + vertsWritten}/{indicies.Item1 + vertsWritten}/{indicies.Item1 + vertsWritten}");
                        sb.Append($" {indicies.Item2 + vertsWritten}/{indicies.Item2 + vertsWritten}/{indicies.Item2 + vertsWritten}");
                        sb.Append($" {indicies.Item3 + vertsWritten}/{indicies.Item3 + vertsWritten}/{indicies.Item3 + vertsWritten}");

                        sb.AppendLine("");
                    }
                }

                sb.AppendLine();

                vertsWritten += verts.Length;
            }

            return sb.ToString();
        }
    }
}
