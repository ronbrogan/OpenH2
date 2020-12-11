using OpenH2.Core.Factories;
using OpenH2.Core.Maps.Vista;
using OpenH2.Core.Tags;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenH2.TagFeatureAnalysis
{
    class Program
    {
        public static Dictionary<uint, bool> LabelLookup = new Dictionary<uint, bool>
        {
            { 0, false },
            {3782868994,true},
            {3895787556,true},
            {3896508462,true},
            {3897163832,true},
            {3897950276,true},
            {3898277961,true},
            {3898605646,true},
            {3898933329,true},
            {3899654236,true},
            {3899850847,true},
            {3900047458,true},
            {3901358198,false},
            {3902013567,false},
            {3902341252,false},
            {3902865546,false},
            {3903520916,false},
            {3903848601,false},
            {3904176286,true},
            {3904831656,true},
            {3905618100,true},
            {3907125448,false},
            {3907453133,false},
            {3908108503,true},
            {3908567262,true},
            {3909222632,true},
            {3910533367,true},
            {3910861052,true},
            {3912302849,true},
            {3912630534,true},
            {3913416971,true},
            {3917349157,true},
            {3918528820,false},
            {3918856505,false},
            {3919184190,false},
            {3919511875,false},
            {3919905097,false},
            {3920888148,true},
            {3958964621,true},
            {3959751064,true},
            {3960471971,true},
            {3961455026,true},
            {3962044859,true}
        };

        static void Main(string[] args)
        {
#if !DEBUG
            throw new Exception("This tool relies on DEBUG functionality, run in DEBUG mode.");
#endif

            if (args.Length != 1)
            {
                throw new Exception("Only 1 argument is accepted");
            }

            var factory = new MapFactory(Path.GetDirectoryName(args[0]));
            var h2map = factory.Load(Path.GetFileName(args[0]));

            if (h2map is not H2vMap map)
            {
                throw new NotSupportedException("Only Vista maps are supported");
            }

            var models = map.GetLocalTagsOfType<RenderModelTag>();
            var containers = new List<ModelContainer>();
            var tagHeaderSize = 92;


            foreach (var model in models)
            {
                var bytes = new byte[72];
                var container = model.Parts[0];

                var partResource = container.Resources[0];
                var partData = partResource.Data.Span;
                
                Span<byte> data = partData;
                for (var i = 0; i < bytes.Length; i++)
                {
                    bytes[i] = data[i];
                }

                containers.Add(new ModelContainer()
                {
                    isTriangleStrip = LabelLookup[model.Id],
                    model = model,
                    features = bytes.Cast<object>()
                });

                var bb = model.BoundingBoxes[0];
                Console.WriteLine($"{model.Name},{{{model.Id},{model.Flags & 0xFF},{model.Flags >> 8}}}");
            }

            Console.ReadLine();
            var file = new FileStream("featuredata.py", FileMode.Create);
            var writer = new StreamWriter(file);

            writer.WriteLine("X = [");
            foreach (var container in containers)
            {
                writer.Write("[");
                foreach (var feature in container.features)
                {
                    writer.Write(feature.ToString());
                    writer.Write(",");
                }

                writer.Flush();
                file.Position -= 1;
                writer.Write("]");
                writer.Write(",");
                writer.WriteLine();
            }

            writer.Flush();
            file.Position -= (1 + Environment.NewLine.Length);
            writer.WriteLine();
            writer.WriteLine("]");

            writer.WriteLine("y = [");
            foreach (var container in containers)
            {
                writer.Write(container.isTriangleStrip ? "True" : "False");
                writer.Write(",");
            }

            writer.Flush();
            file.Position -= 1;
            writer.WriteLine();
            writer.WriteLine("]");

            writer.Close();


        }
    }

    class ModelContainer
    {
        public bool isTriangleStrip;
        public RenderModelTag model;
        public IEnumerable<object> features;
    }
}