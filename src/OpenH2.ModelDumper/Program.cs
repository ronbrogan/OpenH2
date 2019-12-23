using OpenH2.Core.Factories;
using OpenH2.Core.Formats;
using OpenH2.Core.Representations;
using OpenH2.Core.Tags;
using System;
using System.IO;
using System.Linq;
using System.Numerics;

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

            var writer = new WavefrontObjWriter(Path.GetFileNameWithoutExtension(mapPath));

            var bsps = scene.GetLocalTagsOfType<BspTag>();
            var bspMeshes = bsps
                .SelectMany(b => b.RenderChunks);

            foreach(var chunk in bspMeshes)
            { 
                writer.WriteModel(chunk.Model, default, "bsp");
            }

            var instancedGeometries = bsps.SelectMany(b => b.InstancedGeometryInstances
                .Select(i => new { Bsp = b, Instance = i }));

            foreach (var geom in instancedGeometries)
            {
                var def = geom.Bsp.InstancedGeometryDefinitions[geom.Instance.Index];

                var xform = Matrix4x4.CreateScale(new Vector3(geom.Instance.Scale))
                    * Matrix4x4.CreateFromQuaternion(QuatFrom3x3Mat4(geom.Instance.RotationMatrix)) 
                    * Matrix4x4.CreateTranslation(geom.Instance.Position);

                writer.WriteModel(def.Model, xform, "instanced_" + geom.Instance.Index);
            }

            var scenario = scene.GetLocalTagsOfType<ScenarioTag>().First();

            foreach(var blocInstance in scenario.BlocInstances)
            {
                var def = scenario.BlocDefinitions[blocInstance.BlocDefinitionIndex];

                var xform = Matrix4x4.CreateScale(new Vector3(1))
                    * Matrix4x4.CreateFromQuaternion(Quaternion.CreateFromYawPitchRoll(blocInstance.Orientation.Y, blocInstance.Orientation.Z, blocInstance.Orientation.X))
                    * Matrix4x4.CreateTranslation(blocInstance.Position);

                if(!scene.TryGetTag(def.Bloc, out var bloc))
                {
                    continue;
                }

                if (!scene.TryGetTag(bloc.PhysicalModel, out var phmo))
                {
                    continue;
                }

                if (!scene.TryGetTag(phmo.Model, out var mode))
                {
                    continue;
                }

                var part = mode.Lods[0].Permutations[0].HighestPieceIndex;

                writer.WriteModel(mode.Parts[part].Model, xform, "bloc_" + blocInstance.BlocDefinitionIndex);
            }

            foreach (var scenInstance in scenario.SceneryInstances)
            {
                var def = scenario.SceneryDefinitions[scenInstance.SceneryDefinitionIndex];

                var xform = Matrix4x4.CreateScale(new Vector3(1))
                    * Matrix4x4.CreateFromQuaternion(Quaternion.CreateFromYawPitchRoll(scenInstance.Orientation.Y, scenInstance.Orientation.Z, scenInstance.Orientation.X))
                    * Matrix4x4.CreateTranslation(scenInstance.Position);

                if (!scene.TryGetTag(def.Scenery, out var scen))
                {
                    continue;
                }

                if (!scene.TryGetTag(scen.PhysicalModel, out var phmo))
                {
                    continue;
                }

                if (!scene.TryGetTag(phmo.Model, out var mode))
                {
                    continue;
                }

                writer.WriteModel(mode.Parts[0].Model, xform, "scen_" + scenInstance.SceneryDefinitionIndex);
            }

            foreach (var machInstance in scenario.MachineryInstances)
            {
                var def = scenario.MachineryDefinitions[machInstance.MachineryDefinitionIndex];

                var xform = Matrix4x4.CreateScale(new Vector3(1))
                    * Matrix4x4.CreateFromQuaternion(Quaternion.CreateFromYawPitchRoll(machInstance.Orientation.Y, machInstance.Orientation.Z, machInstance.Orientation.X))
                    * Matrix4x4.CreateTranslation(machInstance.Position);

                if (!scene.TryGetTag(def.Machinery, out var mach))
                {
                    continue;
                }

                if (!scene.TryGetTag(mach.PhysicalModel, out var phmo))
                {
                    continue;
                }

                if (!scene.TryGetTag(phmo.Model, out var mode))
                {
                    continue;
                }

                writer.WriteModel(mode.Parts[0].Model, xform, "mach_" + machInstance.MachineryDefinitionIndex);
            }

            File.WriteAllText(outPath, writer.ToString());

            Console.WriteLine($"Processed {Path.GetFileName(mapPath)}");
            Console.WriteLine("Press any key to exit");
            Console.ReadLine();
        }

        private static Quaternion QuatFrom3x3Mat4(float[] vals)
        {
            var mat4 = new Matrix4x4(
                vals[0],
                vals[1],
                vals[2],
                0f,
                vals[3],
                vals[4],
                vals[5],
                0f,
                vals[6],
                vals[7],
                vals[8],
                0f,
                0f,
                0f,
                0f,
                1f);

            if (Matrix4x4.Decompose(mat4, out var _, out var q, out var _) == false)
            {
                return Quaternion.Identity;
            }

            return q;
        }
    }
}
