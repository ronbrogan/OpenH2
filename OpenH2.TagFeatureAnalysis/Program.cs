using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Accord.Statistics.Distributions.Univariate;
using Accord.Statistics.Testing;
using OpenH2.Core.Extensions;
using OpenH2.Core.Factories;
using OpenH2.Core.Tags;

namespace OpenH2.TagFeatureAnalysis
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                throw new Exception("Only 1 argument is accepted");
            }
            
#if DEBUG
#else
                throw new Exception("This tool relies on DEBUG functionality, run in DEBUG mode.")
#endif
            
            var factory = new MapFactory(Path.GetDirectoryName(args[0]));
            var map = factory.FromFile(File.OpenRead(args[0]));

            var models = map.GetLocalTagsOfType<ModelTag>();
            var containers = new List<ModelContainer>();
            var tagHeaderSize = 92;
            
            
            foreach (var model in models)
            {
                bool hasCompressedParts = false;
                var flags = new List<ushort>();

                foreach (var part in model.Parts)
                {
                    var hasMaxX = false;
                    var hasMaxY = false;
                    var hasMaxZ = false;
                    var hasMinX = false;
                    var hasMinY = false;
                    var hasMinZ = false;
                    
                    foreach (var mesh in part.Model.Meshes)
                    {
                        if (mesh.Verticies.Any(v => v.Position.X == 1f))
                        {
                            hasMaxX = true;
                        }
                        
                        if (mesh.Verticies.Any(v => v.Position.Y == 1f))
                        {
                            hasMaxY = true;
                        }
                        
                        if (mesh.Verticies.Any(v => v.Position.Z == 1f))
                        {
                            hasMaxZ = true;
                        }
                        
                        if (mesh.Verticies.Any(v => v.Position.X == -1f))
                        {
                            hasMinX = true;
                        }
                        
                        if (mesh.Verticies.Any(v => v.Position.Y == -1f))
                        {
                            hasMinY = true;
                        }
                        
                        if (mesh.Verticies.Any(v => v.Position.Z == -1f))
                        {
                            hasMinZ = true;
                        }
                    }
                    
                    var isCompressed = hasMaxX && hasMaxY && hasMaxZ && hasMinX && hasMinY && hasMinZ;
                    
                    flags.Add(part.Flags);

                    hasCompressedParts = hasCompressedParts || isCompressed;

                    var features = new BitArray(part.RawData);

                    containers.Add(new ModelContainer()
                    {
                        isCompressed = isCompressed,
                        part = part,
                        features = features
                    });
                }

                var bb = model.BoundingBoxes[0];
                Console.WriteLine($"{model.Name} Compressed:{hasCompressedParts} - BB:{bb.MinX},{bb.MinY},{bb.MinZ}->{bb.MaxX},{bb.MaxY},{bb.MaxZ}");
                
            }

            var file = new FileStream("featuredata.py", FileMode.Create);
            var writer = new StreamWriter(file);

            writer.WriteLine("X = [");
            foreach (var container in containers)
            {
                writer.Write("[");
                foreach (bool feature in container.features)
                {
                    writer.Write(feature ? 1 : 0);
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
                writer.Write(container.isCompressed ? "True": "False");
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
        public bool isCompressed;
        public ModelTag.Part part;
        public BitArray features;
    }
}