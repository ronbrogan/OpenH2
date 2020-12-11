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
            var mapPath = @"D:\H2vMaps\03a_oldmombasa.map";

            var factory = new MapFactory(Path.GetDirectoryName(mapPath));
            var h2map = factory.Load(Path.GetFileName(mapPath));

            if (h2map is not H2vMap map)
            {
                throw new NotSupportedException("Only Vista maps are supported in this tool");
            }

            var animation = map.GetTag<AnimationGraphTag>(4070576426);
            var animations = map.GetLocalTagsOfType<AnimationGraphTag>();

            //Write($"Animation Tags: {animations.Count}");
            Write($"Animation Tracks: {animation.Animations.Count()}");

            animations.WriteInfo(a => a.Bones.Length);
            animations.WriteInfo(a => a.Animations.Length);
            animations.WriteInfo(a => a.Sounds.Length);
            animations.SelectMany(a => a.Animations).WriteInfo(t => t.FrameCount);
            animations.SelectMany(a => a.Animations).WriteInfo(t => t.BoneCount);


            animations.SelectMany(a => a.Animations).WriteInfo(t => t.AnimationType);
            animations.SelectMany(a => a.Animations).WriteInfo(t => t.ValueC);
            animations.SelectMany(a => a.Animations).WriteInfo(t => t.ValueD);
            animations.SelectMany(a => a.Animations).WriteInfo(t => t.ValueE);
            animations.SelectMany(a => a.Animations).WriteInfo(t => t.ValueF);
            animations.SelectMany(a => a.Animations).WriteInfo(t => t.ValueG);
            animations.SelectMany(a => a.Animations).WriteInfo(t => t.ValueH);
            animations.SelectMany(a => a.Animations).WriteInfo(t => t.ValueO);

            for (var i = 0; i < 32; i++)
            {
                var trackType = typeof(AnimationGraphTag.Animation);
                var arg = Expression.Parameter(trackType, "t");
                var access = Expression.ArrayAccess(
                    Expression.MakeMemberAccess(arg, trackType.GetProperty(nameof(AnimationGraphTag.Animation.Data))), Expression.Constant(i));

                var lambda = Expression.Lambda<Func<AnimationGraphTag.Animation, byte>>(access, arg);

                //animations.SelectMany(a => a.Tracks).WriteInfo(lambda);
            }

            TextCopy.ClipboardService.SetText(outb.ToString());
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

