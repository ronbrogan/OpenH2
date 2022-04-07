using OpenH2.Core.Factories;
using OpenH2.Core.Maps.Vista;
using System;
using System.IO;

namespace OpenH2.TriggerAnalysis
{
    class Program
    {
        static void Main(string[] args)
        {
            var maps = Directory.GetFiles(@"D:\H2vMaps", "*.map");

            foreach (var map in maps)
            {
                var factory = new MapFactory(Path.GetDirectoryName(map));
                var h2map = factory.Load(Path.GetFileName(map));

                if (h2map is not H2vMap scene)
                {
                    throw new NotSupportedException("Only Vista maps are supported");
                }

                Console.WriteLine("Checking " + map);

                foreach(var v in scene.Scenario.TriggerVolumes)
                {
                    var vect = v.Orientation;
                    var vect2 = v.OrientationAxis;


                    if (Math.Abs(vect.Length() - 1) > 0.001f)
                    {
                        Console.WriteLine($"Invalid Vector: {v.Description}/orientation, len: {vect.Length()} - values {vect.X},{vect.Y},{vect.Z}");
                    }

                    if(vect.Z > 0.001f)
                    {
                        Console.WriteLine($"NonZero Z component {v.Description}/orientation, len: {vect.Length()} - values {vect.X},{vect.Y},{vect.Z}");
                        Console.WriteLine($"            /something, len: {vect2.Length()} - values {vect2.X},{vect2.Y},{vect2.Z}");
                    }

                    if (Math.Abs(vect2.Length() - 1) > 0.001f)
                    {
                        Console.WriteLine($"Invalid Vector: {v.Description}/something, len: {vect2.Length()} - values {vect2.X},{vect2.Y},{vect2.Z}");
                    }


                }
            }

            Console.WriteLine("done");
            Console.ReadLine();
        }
    }
}
