using OpenH2.Core.Factories;
using OpenH2.Core.ExternalFormats;
using OpenH2.Core.Tags;
using OpenH2.Core.Tags.Common.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using OpenH2.Core.Maps.Vista;

namespace OpenH2.PhysicsModelAnalysis
{
    class Program
    {
        static void Main(string[] args)
        {
            var mapPath = @"D:\H2vMaps\zanzibar.map";

            var factory = new MapFactory(Path.GetDirectoryName(mapPath));
            var h2map = factory.Load(Path.GetFileName(mapPath));

            if (h2map is not H2vMap map)
            {
                throw new NotSupportedException("Only Vista maps are supported");
            }

            var phmos = map.GetLocalTagsOfType<PhysicsModelTag>();

            var matRefs = new HashSet<ushort>();

            foreach(var phmo in phmos)
            {
                foreach (var c in phmo.CapsuleDefinitions)
                    matRefs.Add(c.MaterialIndexMaybe);

                foreach (var c in phmo.BoxDefinitions)
                    matRefs.Add(c.MaterialIndexMaybe);

                foreach (var c in phmo.PolyhedraDefinitions)
                    matRefs.Add(c.MaterialIndexMaybe);
            }

            var meshWithoutPlanes = false;

            foreach(var phmo in phmos)
            {
                if(phmo.PolyhedraDefinitions.Length > 0 && phmo.ColliderPlanes.Length == 0)
                {
                    meshWithoutPlanes = true;
                }
            }

            Console.WriteLine($"Analyzed {phmos.Count()} models");
            Console.WriteLine(string.Join(",", matRefs));
            Console.WriteLine("Meshes imply planes: " + !meshWithoutPlanes);
            

            if (map.TryGetTag<PhysicsModelTag>(3961848248u, out var phymod) == false)
                return;


            var builder = new StringBuilder();
            builder.AppendLine("o CollisionPlanes");

            var currentVert = 0;

            foreach(var plane in phymod.ColliderPlanes)
            {
                var centroid = Vector3.Multiply(plane.Normal, plane.Distance);

                // Use arbitrary vector to get tangent vector to normal
                var tempVec = Vector3.Normalize(new Vector3(plane.Normal.X + 1, plane.Normal.Y, plane.Normal.Z));
                var tangent = Vector3.Normalize(Vector3.Cross(plane.Normal, tempVec));
                var bitangent = Vector3.Cross(plane.Normal, tangent);

                var upperRight = centroid + tangent + bitangent;
                var lowerRight = centroid - tangent + bitangent;
                var upperLeft = centroid + tangent - bitangent;
                var lowerLeft = centroid - tangent - bitangent;

                builder.AppendLine($"v {upperRight.X.ToString("0.000000")} {upperRight.Y.ToString("0.000000")} {upperRight.Z.ToString("0.000000")}");
                builder.AppendLine($"v {lowerRight.X.ToString("0.000000")} {lowerRight.Y.ToString("0.000000")} {lowerRight.Z.ToString("0.000000")}");
                builder.AppendLine($"v {lowerLeft.X.ToString("0.000000")} {lowerLeft.Y.ToString("0.000000")} {lowerLeft.Z.ToString("0.000000")}");
                builder.AppendLine($"v {upperLeft.X.ToString("0.000000")} {upperLeft.Y.ToString("0.000000")} {upperLeft.Z.ToString("0.000000")}");

                builder.AppendLine($"f {++currentVert} {++currentVert} {++currentVert} {++currentVert}");
            }

            builder.AppendLine("o MeshBoxes");

            foreach(var def in phymod.PolyhedraDefinitions)
            {
                for(var i = 0; i < 3; i++)
                {
                    var upperRight = new Vector3(def.FloatsC[i * 12 + 0], def.FloatsC[i * 12 + 1], def.FloatsC[i * 12 + 2]);
                    var lowerRight = new Vector3(def.FloatsC[i * 12 + 3], def.FloatsC[i * 12 + 4], def.FloatsC[i * 12 + 5]);
                    var upperLeft = new Vector3(def.FloatsC[i * 12 + 6], def.FloatsC[i * 12 + 7], def.FloatsC[i * 12 + 8]);
                    var lowerLeft = new Vector3(def.FloatsC[i * 12 + 9], def.FloatsC[i * 12 + 10], def.FloatsC[i * 12 + 11]);

                    builder.AppendLine($"v {upperRight.X.ToString("0.000000")} {upperRight.Y.ToString("0.000000")} {upperRight.Z.ToString("0.000000")}");
                    builder.AppendLine($"v {lowerRight.X.ToString("0.000000")} {lowerRight.Y.ToString("0.000000")} {lowerRight.Z.ToString("0.000000")}");
                    builder.AppendLine($"v {lowerLeft.X.ToString("0.000000")} {lowerLeft.Y.ToString("0.000000")} {lowerLeft.Z.ToString("0.000000")}");
                    builder.AppendLine($"v {upperLeft.X.ToString("0.000000")} {upperLeft.Y.ToString("0.000000")} {upperLeft.Z.ToString("0.000000")}");

                    builder.AppendLine($"f {++currentVert} {++currentVert} {++currentVert} {++currentVert}");
                }
            }

            File.WriteAllText(@"D:\collisionPlanes.obj", builder.ToString());

            Console.ReadLine();
        }
    }
}
