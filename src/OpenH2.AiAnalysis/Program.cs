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
            var mapPath = @"D:\H2vMaps\03b_newmombasa.map";

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


            WriteMaxOn(map, s => s.ValueA);
            WriteMaxOn(map, s => s.ValueB);
            WriteMaxOn(map, s => s.ValueC);
            WriteMaxOn(map, s => s.ValueD);
            WriteMaxOn(map, s => s.ValueE);
            WriteMaxOn(map, s => s.ValueF);
            WriteMaxOn(map, s => s.ValueG);
            WriteMaxOn(map, s => s.AiOrderIndex);

            var locs = map.Scenario.AiSquadDefinitions.SelectMany(d => d.StartingLocations);
            WriteMaxOn(locs, l => l.ZeroOrMax);
            WriteMaxOn(locs, l => l.Zero);
            WriteMaxOn(locs, l => l.Index0);
            WriteMaxOn(locs, l => l.Index1);
            WriteMaxOn(locs, l => l.Flags);
            WriteMaxOn(locs, l => l.Zero2);
            WriteMaxOn(locs, l => l.Index4);
            WriteMaxOn(locs, l => l.Index5);
            WriteMaxOn(locs, l => l.Index6);
            WriteMaxOn(locs, l => l.Zero3);
            WriteMaxOn(locs, l => l.Index8);
            WriteMaxOn(locs, l => l.Index9);
            WriteMaxOn(locs, l => l.Zero4);
            WriteMaxOn(locs, l => l.Zero5);
            WriteMaxOn(locs, l => l.Index12);
            WriteMaxOn(locs, l => l.Index13);
            WriteMaxOn(locs, l => l.Index14);
            WriteMaxOn(locs, l => l.Index15);
            WriteMaxOn(locs, l => l.Zero6);
            WriteMaxOn(locs, l => l.Index17);
            WriteMaxOn(locs, l => l.MaxValue);
            WriteMaxOn(locs, l => l.Zero7);

            TextCopy.ClipboardService.SetText(outb.ToString());
            Console.Read();
        }

        private static void WriteMaxOn<TProp>(H2vMap map, Expression<Func<ScenarioTag.AiSquadDefinition, TProp>> accessor)
        {
            var vals = map.Scenario.AiSquadDefinitions;

            var func = accessor.Compile();
            var maxVal = vals.Max(func);
            var withoutMax = vals.Where(v => func(v).Equals(maxVal) == false);
            var nextMax = maxVal;
            if(withoutMax.Any())
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
        }

        private static void WriteMaxOn<TProp>(IEnumerable<ScenarioTag.AiSquadDefinition.StartingLocation> vals, Expression<Func<ScenarioTag.AiSquadDefinition.StartingLocation, TProp>> accessor)
            where TProp : IEquatable<TProp>
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
