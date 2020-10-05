using Microsoft.VisualBasic.CompilerServices;
using OpenH2.Core.Factories;
using OpenH2.Core.Representations;
using OpenH2.Core.Tags.Scenario;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace OpenH2.AiAnalysis
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

            var factory = new MapFactory(Path.GetDirectoryName(mapPath), NullMaterialFactory.Instance);

            var map = factory.FromFile(File.OpenRead(mapPath));

            var props = typeof(ScenarioTag).GetProperties();
            var longestName = props.Max(p => p.Name.Length);

            foreach (var p in props)
            {
                var pVal = p.GetValue(map.Scenario) as object[];

                if(pVal != null)
                {
                    Write($"Scenario.{p.Name}.Length {("= " + pVal.Length).PadLeft(longestName-p.Name.Length+3)}");
                }
            }

            Write();


            WriteInfo(map, s => s.ActorType);
            WriteInfo(map, s => s.ValueE);
            WriteInfo(map, s => s.ValueF);
            WriteInfo(map, s => s.ValueG);

            var locs = map.Scenario.AiSquadDefinitions.SelectMany(d => d.StartingLocations);
            WriteInfo(locs, l => l.ZeroOrMax);
            WriteInfo(locs, l => l.Zero);
            WriteInfo(locs, l => l.Index0);
            WriteInfo(locs, l => l.Index1);
            WriteInfo(locs, l => l.Flags);
            WriteInfo(locs, l => l.Zero2);
            WriteInfo(locs, l => l.Index4);
            WriteInfo(locs, l => l.Index5);
            WriteInfo(locs, l => l.Index6);
            WriteInfo(locs, l => l.Zero3);
            WriteInfo(locs, l => l.Index8);
            WriteInfo(locs, l => l.State);
            WriteInfo(locs, l => l.Zero4Sometimes);
            WriteInfo(locs, l => l.Zero5Sometimes);
            WriteInfo(locs, l => l.Index12);
            WriteInfo(locs, l => l.Index13);
            WriteInfo(locs, l => l.Index14);
            WriteInfo(locs, l => l.Index15);
            WriteInfo(locs, l => l.Zero6);
            WriteInfo(locs, l => l.Index17);
            WriteInfo(locs, l => l.MaxValue);
            WriteInfo(locs, l => l.Zero7Sometimes);

            TextCopy.ClipboardService.SetText(outb.ToString());
            Console.Read();
        }

        private static void WriteInfo<TProp>(H2vMap map, Expression<Func<ScenarioTag.AiSquadDefinition, TProp>> accessor)
        {
            var vals = map.Scenario.AiSquadDefinitions;

            WriteInfo(vals, accessor);
        }

        private static void WriteInfo<TEnum, TProp>(IEnumerable<TEnum> vals, Expression<Func<TEnum, TProp>> accessor)
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

            Write($"\t\t{string.Join(",", vals.Select(func).Distinct())}");
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
