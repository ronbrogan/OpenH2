using OpenH2.Core.Factories;
using OpenH2.Core.Maps;
using OpenH2.Core.Tags;
using OpenBlam.Serialization.Materialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Text;
using OpenH2.Core.Maps.Vista;
using OpenH2.Core.Animation;

namespace OpenH2.AnimationAnalysis
{
    public static class Program
    {
        static StringBuilder outb = new StringBuilder();
        public static void Write(string s = null)
        {
            Console.WriteLine(s);
            outb.AppendLine(s);
        }

        static void Main(string[] args)
        {
            var mapPath = @"D:\H2vMaps\01a_tutorial.map";
            var outRoot = @"D:\h2scratch\animations\export";

            var factory = new MapFactory(Path.GetDirectoryName(mapPath));
            var h2map = factory.Load(Path.GetFileName(mapPath));

            if (h2map is not H2vMap map)
            {
                throw new NotSupportedException("Only Vista maps are supported in this tool");
            }

            var proc = JmadDataProcessor.GetProcessor();
            var animations = map.GetLocalTagsOfType<AnimationGraphTag>();

            var types = new HashSet<JmadDataType>();

            foreach(var tag in animations)
            {
                foreach(var anim in tag.Animations)
                {
                    Write($"{tag.Name} {anim.Description}, T{anim.AnimationType}, F{anim.FrameCount}, B{anim.BoneCount}");

                    Span<byte> data = anim.Data;

                    var zeroHeader = proc.ReadHeader(data);
                    if(zeroHeader.Type != JmadDataType.Flat)
                    {
                        Write(zeroHeader);
                        types.Add(zeroHeader.Type);
                    }

                    var innerHeader = proc.ReadHeader(data.Slice(anim.SizeE));
                    if (innerHeader.Type != JmadDataType.Flat)
                    {
                        Write(innerHeader);
                        types.Add(innerHeader.Type);
                    }

                    var outFile = Path.Combine(outRoot, tag.Name, anim.Description.Replace(":", "-")) + ".jmad";
                    Directory.CreateDirectory(Path.GetDirectoryName(outFile));
                    File.WriteAllBytes(outFile, anim.Data);
                    
                    Write("=====================");
                }
            }

            Write($" Encountered Data Types: {string.Join(",", types)}");

            TextCopy.ClipboardService.SetText(outb.ToString());

            
        }

        static void Write(JmadDataHeader header)
        {
            Write($"Type: {header.Type}");
            Write($"  OrientationCount: {header.OrientationCount}");
            Write($"  TranslationCount: {header.TranslationCount}");
            Write($"  ScaleCount: {header.ScaleCount}");
        }

        private static void WriteInfo<TItem, TProp>(this TItem val, Expression<Func<TItem, TProp>> accessor)
        {
            var func = accessor.Compile();
            
            var accessorval = func(val);

            Write($"{accessor}: {accessorval}");
        }

        private static void WriteInfo<TEnum, TProp>(this IEnumerable<TEnum> vals, Expression<Func<TEnum, TProp>> accessor)
        {
            var func = accessor.Compile();
            var maxVal = vals.Max(func);
            var withoutMax = vals.Where(v => func(v).Equals(maxVal) == false);
            var nextMax = maxVal;
            if (withoutMax.Any())
            {
                nextMax = withoutMax.Max(func);
            }
            var minVal = vals.Min(func);
            var nextMin = minVal;
            var withoutMin = vals.Where(v => func(v).Equals(minVal) == false);
            if (withoutMin.Any())
            {
                nextMin = withoutMin.Min(func);
            }
            var avgVal = vals.Avg(accessor);

            Write($"{accessor}: [{minVal}, {nextMin}, {nextMax}, {maxVal}] {avgVal}");

            var distinct = vals.Select(func).Distinct().ToArray();

            if(distinct.Length < 45)
            {
                Write($"\t\t{string.Join(",", distinct)}");
            }
            else
            {
                Write($"\t\tDistinct Count: {distinct.Length}");
            }
        }

        public static object Avg<TEnum, TProp>(this IEnumerable<TEnum> source, Expression<Func<TEnum, TProp>> accessor)
        {
            var defVal = default(TProp);
            double avg = defVal switch
            {
                float f => source.Average(Cast<Func<TEnum, float>>(typeof(float))),
                double f => source.Average(Cast<Func<TEnum, double>>(typeof(double))),
                int f => source.Average(Cast<Func<TEnum, int>>(typeof(int))),
                uint f => source.Average(Cast<Func<TEnum, long>>(typeof(long))),
                short f => source.Average(Cast<Func<TEnum, int>>(typeof(int))),
                ushort f => source.Average(Cast<Func<TEnum, int>>(typeof(int))),
                byte f => source.Average(Cast<Func<TEnum, int>>(typeof(int)))
            };

            return avg.ToString("n");

            TDelegate Cast<TDelegate>(Type t)
            {
                var val = Expression.Convert(accessor.Body, t);

                return Expression.Lambda<TDelegate>(val, accessor.Parameters).Compile();
            }
        }
    }
}

