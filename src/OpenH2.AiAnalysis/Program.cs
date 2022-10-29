using OpenH2.Core.Factories;
using OpenH2.Core.Maps.Vista;
using OpenH2.Core.Tags;
using OpenH2.Core.Tags.Scenario;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography.X509Certificates;
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

        private static Dictionary<string, (int max, int[])> AllowedCharacters = new()
        {
            ["01a_tutorial"] = (4, new[] { 0, 1, 2, 3, 4 }),
            ["01b_spacestation"] = (19, new[] { 0, 1, 2, 4, 5, 6, 7, 9, 10, 11, 12, 14, 15, 16, 17, 18 }),
            ["03a_oldmombasa"] = (14, new[] { 0, 1, 2, 3, 4, 5, 7, 8, 9, 10, 11, 12, 13, 14 }),
            ["03b_newmombasa"] = (14, new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 10, 11, 12, 13, 14 }),
            ["04a_gasgiant"] = (5, new[] { 0, 1, 2, 3, 5 }),
            ["04b_floodlab"] = (12, new[] { 0, 1, 2, 3, 4, 6, 8, 11 }),
            ["05a_deltaapproach"] = (12, new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }),
            ["05b_deltatowers"] = (12, new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }),
            ["06a_sentinelwalls"] = (19, new[] { 0, 2, 3, 4, 5, 7, 9, 10, 11, 14, 15, 16, 17, 18, 19 }),
            ["06b_floodzone"] = (11, new[] { 1, 2, 3, 4, 6, 7, 8, 9, 10, 11 }),
            ["07a_highcharity"] = (27, new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 22, 23, 24, 25 }),
            ["07b_forerunnership"] = (17, new[] { 0, 1, 2, 4,/*jugg*/ 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 }),
            ["08a_deltacliffs"] = (11, new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 }),
            ["08b_deltacontrol"] = (19, new[] { 0, 1, 2, 3, 4, 5, 6, 8, 10, 11, 14, 15, 17, 18, 19 }),
        };

        // These have a separate max, because the max in our allowed may not be the same, and there
        // needs to be enough in the check array at runtime for all possible options
        private static Dictionary<string, (int max, int[])> Weapons = new()
        {
            ["01a_tutorial"] = (0, new[] { 0 }),
            ["01b_spacestation"] = (10, new[] { 0, 1, 2, 3, 4, 5, 8, 9 }),
            ["03a_oldmombasa"] = (16, new[] { 0, 1, 2, 3, 4, 5, 6, 8, 9, 10, 14 }),
            ["03b_newmombasa"] = (14, new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 13 }),
            ["04a_gasgiant"] = (10, new[] { 0, 1, 2, 3, 4, 5, 6, 7 }),
            ["04b_floodlab"] = (6, new[] { 0, 1, 2, 3, 4, 5 }),
            ["05a_deltaapproach"] = (14, new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }),
            ["05b_deltatowers"] = (15, new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 15 }),
            ["06a_sentinelwalls"] = (14, new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 11, 13 }),
            ["06b_floodzone"] = (12, new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 11 }),
            ["07a_highcharity"] = (12, new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }),
            ["07b_forerunnership"] = (16, new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 14, 15 }),
            ["08a_deltacliffs"] = (11, new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }),
            ["08b_deltacontrol"] = (12, new[] { 0, 1, 2, 3, 5, 6, 7, 8, 9 })
        };

        static void Main(string[] args)
        {


            var mapPath = @"D:\H2vMaps\03a_oldmombasa.map";
            var factory = new MapFactory(Path.GetDirectoryName(mapPath));

            var characterWeapons = new Dictionary<string, HashSet<string>>();

            var prohibitedWeapons = new HashSet<string>
            {
                "head", "head_sp", "jackal_shield", "scarab_main_gun_handheld", "hunter_particle_cannon", "beam_rifle_noplayer", "gravity_hammer", "big_needler_handheld", "h_turret_mp_item", "c_turret_mp_item", "phantom_turret_handheld", "h_turret_mp_weapon"
            };

            var allWeapons = new HashSet<string>();

            var outPath = @"C:\Users\ronal\source\Rampancy\src\H2Randomizer\Levels";

            //var path = mapPath;
            foreach (var path in Directory.GetFiles(Path.GetDirectoryName(mapPath), "*.map"))
            {
                var h2map = factory.Load(Path.GetFileName(path));
                if (h2map is not H2vMap map)
                {
                    throw new NotSupportedException("Only Vista maps are supported in this tool");
                }

                if (map.Scenario.LevelInfos[0].CampaignInfos.Length == 0)
                    return;

                var mapName = map.Scenario.LevelInfos[0].CampaignInfos[0].EnglishName.Replace(" ", "");

                Console.WriteLine(mapName);
                for (var i = 0; i < map.Scenario.CreatureDefinitions.Length; i++)
                {
                    if (map.TryGetTag<BaseTag>(map.Scenario.CreatureDefinitions[i].Creature, out var crea))
                    {
                        Console.WriteLine(crea.Name);
                    }
                }

                continue;

                //Console.WriteLine(mapName + " weapons");

                var characterWeaponClasses = new Dictionary<uint, HashSet<string>>();


                var weaponsByClass = new Dictionary<string, HashSet<string>>();
                for (var i = 0; i < map.Scenario.WeaponDefinitions.Length; i++)
                {
                    if (map.TryGetTag(map.Scenario.WeaponDefinitions[i].Weapon, out var weap))
                    {
                        if (!weaponsByClass.TryGetValue(weap.WeaponClass, out var weaps))
                        {
                            weaps = new HashSet<string>();
                            weaponsByClass[weap.WeaponClass] = weaps;
                        }

                        weaps.Add(Path.GetFileName(weap.Name));

                        allWeapons.Add(Path.GetFileName(weap.Name));
                    }
                }


                

                foreach (var c in map.Scenario.CharacterDefinitions)
                {
                    if(map.TryGetTag(c.CharacterReference, out var chart))
                    {
                        if(map.TryGetTag<BaseTag>(chart.Unit, out var unit) && unit is BipedTag unitBip)
                        {
                            if(map.TryGetTag(unitBip.Model, out var model))
                            {
                                if(map.TryGetTag(model.AnimationGraph, out var anims))
                                {
                                    var combatAnimations = anims.Animations.Where(a => a.Name.StartsWith("combat:"));
                                    var weaponClasses = combatAnimations.Select(a => a.Name.Split(':')[1]).ToHashSet();

                                    characterWeaponClasses[c.CharacterReference.Id] = weaponClasses;

                                    var name = Path.GetFileName(chart.Name);

                                    if (!characterWeapons.TryGetValue(name, out var weapons))
                                    {
                                        weapons = new HashSet<string>();
                                        characterWeapons[name] = weapons;
                                    }

                                    foreach(var cls in weaponClasses)
                                    {
                                        if(weaponsByClass.TryGetValue(cls, out var weaps))
                                        {
                                            foreach(var w in weaps)
                                                if(!prohibitedWeapons.Contains(w))
                                                    weapons.Add(w);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                var outFile = Path.Combine(outPath, $"{mapName}Data.cs");
                var output = new StringBuilder();
                output.AppendLine($"using System.Collections.Generic;\r\n\r\nusing static H2Randomizer.Levels.{mapName}Characters;\r\nusing static H2Randomizer.Levels.{mapName}Weapons;\r\n\r\nnamespace H2Randomizer.Levels;\r\n");

                output.AppendLine($"public enum {mapName}Characters");
                output.AppendLine("{");
                for (var i = 0; i < map.Scenario.CharacterDefinitions.Length; i++)
                {
                    if (map.TryGetTag(map.Scenario.CharacterDefinitions[i].CharacterReference, out var chart))
                    {
                        output.AppendLine($"\t{Path.GetFileName(chart.Name)} = {i},");
                    }
                }
                output.AppendLine("}");

                output.AppendLine($"public enum {mapName}Weapons");
                output.AppendLine("{");
                for (var i = 0; i < map.Scenario.WeaponDefinitions.Length; i++)
                {
                    if (map.TryGetTag(map.Scenario.WeaponDefinitions[i].Weapon, out var chart))
                    {
                        output.AppendLine($"\t{Path.GetFileName(chart.Name)} = {i},");
                    }
                }
                output.AppendLine("}");


                output.AppendLine($"public class {mapName}Data : BaseLevelData<{mapName}Characters, {mapName}Weapons>");
                output.AppendLine("{");

                if(AllowedCharacters.TryGetValue(map.Header.Name, out var maxAndChars))
                {
                    var chars = string.Join(", ", maxAndChars.Item2.Select(c => {
                        string? name = null;

                        if(map.TryGetTag(map.Scenario.CharacterDefinitions[c].CharacterReference, out var chart))
                        {
                            name = Path.GetFileName(chart.Name);
                        }

                        return name;

                        }).Where(s => !string.IsNullOrWhiteSpace(s)));

                    output.AppendLine($"\tpublic override {mapName}Characters[] ValidCharacters => new[]{{{chars}}};");
                }

                if (Weapons.TryGetValue(map.Header.Name, out var maxAndWeaps))
                {
                    var chars = string.Join(", ", maxAndWeaps.Item2.Select(c => {
                        string? name = null;

                        if (map.TryGetTag(map.Scenario.WeaponDefinitions[c].Weapon, out var chart))
                        {
                            name = Path.GetFileName(chart.Name);
                        }

                        return name;

                    }).Where(s => !string.IsNullOrWhiteSpace(s)));

                    output.AppendLine($"\tpublic override {mapName}Weapons[] ValidWeapons => new[]{{{chars}}};");
                }

                output.AppendLine("}");

                //File.WriteAllText(outFile, output.ToString());
            }

            //foreach (var (character, weaps) in characterWeapons.ToArray().OrderBy(kv => kv.Key))
            //{
            //    if (weaps.Count == 0)
            //        Console.WriteLine($"[\"{character}\"] = Array.Empty<string>(),");
            //    else
            //        Console.WriteLine($"[\"{character}\"] = CharacterWeapons[\"{character}\"].Without(HumanWeapons),");
            //}


            //Console.WriteLine(string.Join(", ", allWeapons.Select(w => $"\"{w}\"")));


            //var props = typeof(ScenarioTag).GetProperties();
            //var longestName = props.Max(p => p.Name.Length);
            //
            //foreach (var p in props)
            //{
            //    var pVal = p.GetValue(map.Scenario) as object[];
            //
            //    if(pVal != null)
            //    {
            //        Write($"Scenario.{p.Name}.Length {("= " + pVal.Length).PadLeft(longestName-p.Name.Length+3)}");
            //    }
            //}
            //
            //Write();
            //
            //for (var i = 0; i < map.Scenario.EquipmentDefinitions.Length; i++)
            //{
            //    map.TryGetTag<BaseTag>(map.Scenario.EquipmentDefinitions[i].Equipment, out var equip);
            //    Write($"EQIP {i}: {equip?.Name ?? "INVALID"}");
            //}
            //
            //for (var i = 0; i < map.Scenario.WeaponDefinitions.Length; i++)
            //{
            //    map.TryGetTag(map.Scenario.WeaponDefinitions[i].Weapon, out var weap);
            //    Write($"WEAP {i}: {weap?.Name ?? "INVALID"}");
            //}
            //
            //for (var i = 0; i < map.Scenario.VehicleDefinitions.Length; i++)
            //{
            //    var tag = map.GetTag(map.Scenario.VehicleDefinitions[i].Vehicle);
            //    Write($"VEHI {i}: {tag.Name}");
            //}
            //
            //for (var i = 0; i < map.Scenario.CharacterDefinitions.Length; i++)
            //{
            //    map.TryGetTag(map.Scenario.CharacterDefinitions[i].CharacterReference, out var tag);
            //    Write($"CHAR {i}: {tag?.Name ?? "INVALID" }");
            //}
            //
            //var slProps = typeof(ScenarioTag.AiSquadDefinition.StartingLocation).GetProperties()
            //    .Where(p => p.PropertyType.IsPrimitive && p.PropertyType != typeof(float));
            //var sqProps = typeof(ScenarioTag.AiSquadDefinition).GetProperties()
            //    .Where(p => p.PropertyType.IsPrimitive && p.PropertyType != typeof(float));
            //
            //foreach (var squad in map.Scenario.AiSquadDefinitions)
            //{
            //    var sqsb = new StringBuilder();
            //
            //    sqsb.Append($"Squad: {squad.Description}");
            //
            //    foreach (var p in sqProps)
            //    {
            //        sqsb.Append($", {p.Name}:{p.GetValue(squad)}");
            //    }
            //
            //    Write(sqsb.ToString());
            //
            //    foreach (var sl in squad.StartingLocations)
            //    {
            //        var sb = new StringBuilder();
            //
            //        sb.Append($"\t> {sl.Description}");
            //
            //        foreach (var p in slProps)
            //        {
            //            sb.Append($", {p.Name}:{p.GetValue(sl)}");
            //        }
            //
            //        Write(sb.ToString());
            //    }
            //}
            //
            //WriteInfo(map, s => s.ValueA);
            //WriteInfo(map, s => s.ValueB);
            //WriteInfo(map, s => s.ValueC);
            //WriteInfo(map, s => s.ValueC2);
            //WriteInfo(map, s => s.ValueC3);
            //WriteInfo(map, s => s.ValueD3);
            //WriteInfo(map, s => s.ValueD4);
            //WriteInfo(map, s => s.ValueF);
            //WriteInfo(map, s => s.ValueG);
            //WriteInfo(map, s => s.ValueI);
            //WriteInfo(map, s => s.ValueJ);
            //
            //var locs = map.Scenario.AiSquadDefinitions.SelectMany(d => d.StartingLocations);
            //WriteInfo(locs, l => l.ZeroOrMax);
            //WriteInfo(locs, l => l.Zero);
            //WriteInfo(locs, l => l.Flags);
            //WriteInfo(locs, l => l.Zero2);
            //WriteInfo(locs, l => l.WeaponIndex);
            //WriteInfo(locs, l => l.Index6);
            //WriteInfo(locs, l => l.Zero3);
            //WriteInfo(locs, l => l.Index8);
            //WriteInfo(locs, l => l.State);
            //WriteInfo(locs, l => l.Zero4Sometimes);
            //WriteInfo(locs, l => l.Index14);
            //WriteInfo(locs, l => l.Index15);
            //WriteInfo(locs, l => l.MaxValue);
            //WriteInfo(locs, l => l.Zero7Sometimes);
            //
            //TextCopy.ClipboardService.SetText(outb.ToString());
            //Console.Read();
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
