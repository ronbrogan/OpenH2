using OpenH2.Core.Factories;
using OpenH2.Core.Tags;
using OpenH2.Serialization.Materialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace OpenH2.AnimationAnalysis
{
    class Program
    {
        static void Main(string[] args)
        {
            var mapPath = @"D:\H2vMaps\03a_oldmombasa.map";

            var mapFactory = new MapFactory(Path.GetDirectoryName(mapPath), NullMaterialFactory.Instance);
            var map = mapFactory.FromFile(File.OpenRead(mapPath));

            var animation = map.GetTag<AnimationGraphTag>(3834577689U);

            foreach(var track in animation.Tracks)
            {
                var frames = track.Values[8];

                Span<byte> data = track.Data;

                var rotStart = 64;
                var posStart = 64 + data.ReadInt32At(52);

                var frameData = new List<(Quaternion, Vector3)>();

                for (int i = 0; i < frames; i++)
                {
                    var quatOffset = i * 8;
                    var thisQuat = rotStart + quatOffset;

                    var quat = new Quaternion(
                        Decompress(data.ReadInt16At(thisQuat + 0)),
                        Decompress(data.ReadInt16At(thisQuat + 2)),
                        Decompress(data.ReadInt16At(thisQuat + 4)),
                        Decompress(data.ReadInt16At(thisQuat + 6))
                    );

                    var posOffset = i * 12;
                    var thisPos = posStart + posOffset;
                    var pos = new Vector3(
                        data.ReadFloatAt(thisPos + 0),
                        data.ReadFloatAt(thisPos + 4),
                        data.ReadFloatAt(thisPos + 8)
                    );

                    frameData.Add((quat, pos));
                }

                Console.WriteLine("All normal: ");
                Console.WriteLine("\t" + frameData.All(d => d.Item1.Length() > 0.9999f && d.Item1.Length() < 1.0001f));
            }

            Console.ReadLine();

            float Decompress(short v) => v / 32768.0f;
        }
    }
}
