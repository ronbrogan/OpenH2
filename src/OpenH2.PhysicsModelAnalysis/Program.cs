using OpenH2.Core.Factories;
using OpenH2.Core.Representations;
using OpenH2.Core.Tags;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenH2.PhysicsModelAnalysis
{
    class Program
    {
        static void Main(string[] args)
        {
            var mapName = @"D:\H2vMaps\zanzibar.map";

            H2vMap map;

            var fac = new MapFactory(Path.GetDirectoryName(mapName), new MaterialFactory(Environment.CurrentDirectory + "\\Configs"));
            using (var fs = new FileStream(mapName, FileMode.Open))
                map = fac.FromFile(fs);

            var phmos = map.GetLocalTagsOfType<PhysicsModelTag>();

            var matRefs = new HashSet<ushort>();

            foreach(var phmo in phmos)
            {
                foreach (var c in phmo.CapsuleDefinitions)
                    matRefs.Add(c.MaterialIndexMaybe);

                foreach (var c in phmo.BoxDefinitions)
                    matRefs.Add(c.MaterialIndexMaybe);

                foreach (var c in phmo.MeshDefinitions)
                    matRefs.Add(c.MaterialIndexMaybe);
            }

            var meshWithoutPlanes = false;

            foreach(var phmo in phmos)
            {
                if(phmo.MeshDefinitions.Length > 0 && phmo.ColliderPlanes.Length == 0)
                {
                    meshWithoutPlanes = true;
                }
            }

            Console.WriteLine($"Analyzed {phmos.Count()} models");
            Console.WriteLine(string.Join(",", matRefs));
            Console.WriteLine("Meshes imply planes: " + !meshWithoutPlanes);
            Console.ReadLine();
        }
    }
}
