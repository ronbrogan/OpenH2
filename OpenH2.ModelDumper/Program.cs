using OpenH2.Core.Factories;
using OpenH2.Core.Representations;
using OpenH2.Core.Tags;
using OpenH2.Core.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OpenH2.ModelDumper
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Must provide 2 arguments: map path, and output directory");
                return;
            }

            var mapPath = args[0];
            var outPath = args[1];

            if (File.Exists(mapPath) == false)
            {
                Console.WriteLine($"Error: Could not find {mapPath}");
                return;
            }

            H2vMap scene = null;

            using (var map = new FileStream(mapPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var factory = new MapFactory(Path.GetDirectoryName(mapPath));
                scene = factory.FromFile(map);
            }

            var processed = 0;

            var models = scene.GetLocalTagsOfType<ModelTag>();

            foreach (var modelTag in models)
            {
                var writePath = Path.Combine(outPath, Path.GetDirectoryName(modelTag.Name));
                if (Directory.Exists(writePath) == false)
                {
                    Directory.CreateDirectory(writePath);
                }

                for(var i = 0; i < modelTag.Parts.Length; i++)
                {
                    var writeName = $"{Path.GetFileName(modelTag.Name)}.{i}";
                    var writePathAndName = Path.Combine(writePath, writeName);

                    Console.WriteLine($"Writing {writeName} to {writePath}");

                    //File.WriteAllBytes(writePathAndName + ".model", modelTag.Parts[i].RawData);
                    File.WriteAllText(writePathAndName + ".obj", CreatObjFileForMesh(modelTag.Parts[0].Mesh));

                    processed++;
                }
            }

            Console.WriteLine($"Processed {processed} models from {Path.GetFileName(mapPath)}");
            Console.WriteLine("Press any key to exit");
            Console.ReadLine();
        }

        public static string CreatObjFileForMesh(ModeMesh mesh)
        {
            var triangles = new List<(int, int, int)>();

            for (int i = 0; i < mesh.Indicies.Length - 2; i++)
            {
                (int, int, int) triangle = default((int, int, int));

                if(i % 2 == 0)
                {
                    triangle = (
                        mesh.Indicies[i] + 1,
                        mesh.Indicies[i + 1] + 1,
                        mesh.Indicies[i + 2] + 1
                    );
                }
                else
                {
                    triangle = (
                        mesh.Indicies[i] + 1,
                        mesh.Indicies[i + 2] + 1,
                        mesh.Indicies[i + 1] + 1
                    );
                }


                triangles.Add(triangle);
            }

            var sb = new StringBuilder();

            foreach(var vert in mesh.Verticies)
            {
                sb.AppendLine($"v {vert.Position.X.ToString("0.000000")} {vert.Position.Y.ToString("0.000000")} {vert.Position.Z.ToString("0.000000")}");
            }

            foreach(var tri in triangles)
            {
                sb.AppendLine($"f {tri.Item1} {tri.Item2} {tri.Item3}");
            }

            return sb.ToString();
        }
    }
}
