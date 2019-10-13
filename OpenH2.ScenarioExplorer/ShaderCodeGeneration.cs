using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace OpenH2.ScenarioExplorer
{
    public static class ShaderCodeGeneration
    {
        private static Regex ParamPattern = new Regex("// {3}(.*) (.*);$", RegexOptions.Compiled | RegexOptions.Multiline);
        private static Regex RegisterPattern = new Regex("// {3}(\\S+)\\s+(\\S+)\\s+(\\d+)\\s+", RegexOptions.Compiled | RegexOptions.Multiline);
        private static Regex CodeLinePattern = new Regex(" {4}(.*)$", RegexOptions.Compiled | RegexOptions.Multiline);


        public static string TranslateAsmShaderToPseudocode(string shaderAsm)
        {
            var paramStart = Math.Max(shaderAsm.IndexOf("//\r\n//\r\n"), 0);
            var registersStart = Math.Max(shaderAsm.IndexOf("//\r\n//\r\n", paramStart + 2), 0);
            var codeStart = Math.Max(shaderAsm.IndexOf("//\r\n\r\n", registersStart + 2), 0);

            var paramLines = ParamPattern.Matches(shaderAsm.Substring(paramStart, registersStart - paramStart));
            var registerLines = RegisterPattern.Matches(shaderAsm.Substring(registersStart, codeStart - registersStart));
            var codeLines = CodeLinePattern.Matches(shaderAsm.Substring(codeStart));

            var parms = new List<ParamEntry>();
            var regs = new List<RegisterEntry>();
            var ops = new List<Operation>();

            foreach (Match p in paramLines)
            {
                parms.Add(ParamEntry.CreateFromGroup(p.Groups));
            }

            foreach (Match r in registerLines)
            {
                regs.Add(RegisterEntry.CreateFromGroup(r.Groups));
            }

            foreach (Match r in codeLines)
            {
                ops.Add(Operation.CreateFromString(r.Groups[1].Value));
            }

            //parms.Dump();
            //regs.Dump();
            //ops.Dump();


            // TODO: Decls and defs

            var pseudo = new StringBuilder();
            var context = new GlslBuilderContext();

            pseudo.AppendLine("#version 450");
            pseudo.AppendLine();

            foreach (var op in ops)
            {
                pseudo.Append(op.ToDeclarationalGlsl(context, parms));
            }

            pseudo.AppendLine();

            foreach (var reg in regs)
            {
                if (context.HasBeenSeen(reg.RegisterId))
                    continue;

                pseudo.AppendLine($"vec4 {reg.RegisterId};");
            }

            pseudo.AppendLine();
            pseudo.AppendLine("void main()");
            pseudo.AppendLine("{");

            foreach (var op in ops)
            {
                pseudo.Append(op.ToExecutableGlsl(context));
            }

            pseudo.AppendLine("}");
            return pseudo.ToString();
        }

        private class ParamEntry
        {
            public string Type { get; set; }
            public string Name { get; set; }

            public static ParamEntry CreateFromGroup(GroupCollection g)
            {
                if (g == null || g.Count < 3)
                {
                    return null;
                }

                return new ParamEntry()
                {
                    Type = g[1].Value,
                    Name = g[2].Value
                };
            }
        }

        private class RegisterEntry
        {

            public string Name { get; set; }
            public string RegisterId { get; set; }
            public int Size { get; set; }

            public static RegisterEntry CreateFromGroup(GroupCollection g)
            {
                if (g == null || g.Count < 4)
                {
                    return null;
                }

                return new RegisterEntry()
                {
                    Name = g[1].Value,
                    RegisterId = g[2].Value,
                    Size = int.Parse(g[3].Value)
                };
            }
        }

        private class GlslBuilderContext
        {
            public int IndentLevel { get; set; } = 1;
            public int IndentSpaces { get; set; } = 4;

            private int loopCount = 0;
            public char GetNewLoopVar()
            {
                var i = (int)'i' + loopCount;
                loopCount++;
                return (char)(i);
            }

            private HashSet<string> seenArgs = new HashSet<string>();
            public bool HasBeenSeen(string arg)
            {
                if (seenArgs.Contains(arg))
                    return true;

                seenArgs.Add(arg);
                return false;
            }

            public Dictionary<string, string> VariableTypes = new Dictionary<string, string>();
        }

        private class Operation
        {
            public string Original { get; set; }
            public string Instruction { get; set; }
            public string Modifier { get; set; }
            public string[] Arguments { get; set; }

            public static Operation CreateFromString(string line)
            {
                var parts = line.Trim().Split(' ').Select(p => p.Trim(','));

                var op = new Operation()
                {
                    Original = line,
                    Instruction = parts.First(),
                    Arguments = parts.Skip(1).ToArray()
                };

                var instructionParts = op.Instruction.Split('_');

                if (instructionParts.Length > 1)
                {
                    op.Instruction = instructionParts[0];
                    op.Modifier = Regex.Replace(instructionParts[1], "^(\\w*?)\\d*$", "$1");
                }

                return op;
            }

            private string GetArgSwizzle(string arg) => arg.Split('.').ElementAtOrDefault(1);
            private string WithoutSwizzle(string arg) => arg.Split('.').First();

            private static Dictionary<string, string> comparisons = new Dictionary<string, string>()
            {
                  { "gt", ">" },
                  { "lt", "<" },
                  { "ge", ">=" },
                  { "le", ">=" },
                  { "ne", "!=" },
                  { "eq", "==" },
             };

            private static Dictionary<string, string> declarationTypes = new Dictionary<string, string>()
            {
                { "2d", "sampler2D" },
                { "volume", "sampler3D" },
                { "cube", "samplerCube" },

                {"binormal", "vec4" },
                {"blendindices", "uint" },
                {"blendweight", "float" },
                {"color", "vec4" },
                {"normal", "vec4" },
                {"position", "vec4" },
                {"positiont", "vec4" },
                {"psize", "float" },
                {"tangent", "vec4" },
                {"texcoord", "vec4" },

                {"fog", "float" },
                {"tessfactor", "float" }
            };

            public string ToDeclarationalGlsl(GlslBuilderContext context, List<ParamEntry> parms)
            {
                var sb = new StringBuilder();



                switch (Instruction)
                {
                    case "dcl":
                        if (context.HasBeenSeen(WithoutSwizzle(Arguments[0])))
                            return sb.ToString();

                        var declType = Modifier != null && declarationTypes.ContainsKey(Modifier) ? declarationTypes[Modifier] : "vec4";

                        if (Modifier == "2d" || Modifier == "volume" || Modifier == "cube")
                            sb.AppendLine($"uniform {declarationTypes[Modifier]} {WithoutSwizzle(Arguments[0])};");
                        else
                            sb.AppendLine($"in {declType} {WithoutSwizzle(Arguments[0])};");

                        context.VariableTypes[WithoutSwizzle(Arguments[0])] = declType;
                        break;

                    case "def":
                        if (context.HasBeenSeen(WithoutSwizzle(Arguments[0])))
                            return sb.ToString();

                        sb.AppendLine($"vec4 {Arguments[0]} = vec4({Arguments[1]}, {Arguments[2]}, {Arguments[3]}, {Arguments[4]});");
                        context.VariableTypes[Arguments[0]] = "vec4";
                        break;
                }

                return sb.ToString();
            }

            public string ToExecutableGlsl(GlslBuilderContext context)
            {
                var sb = new StringBuilder();

                switch (Instruction)
                {
                    case "def":
                    case "dcl":
                        break;

                    case "abs":
                        sb.AppendLine($"{Ind()}{DestArg(0)} = abs({Arguments[1]});");
                        break;

                    case "add":
                        sb.AppendLine($"{Ind()}{DestArg(0)} = ({Arguments[1]} + {Arguments[2]}){ResolveSwizzleForDest(0, 1, 2)};");
                        break;

                    case "break":
                        if (Modifier == null)
                            sb.AppendLine($"{Ind()}break; //" + Original);
                        else
                            sb.AppendLine($"{Ind()}if (!({Arguments[0]} {comparisons[Modifier]} {Arguments[1]})) {{ break; }} //" + Original);
                        break;
                    case "breakp":
                        sb.AppendLine($"{Ind()}if ({Arguments[0]}) {{ break; }} //" + Original);
                        break;

                    case "call":
                        sb.AppendLine($"{Ind()}{Arguments[0]}();");
                        break;
                    case "callnz":
                        sb.AppendLine($"{Ind()}if ({Arguments[1]}) {{ {Arguments[0]}(); }}");
                        break;

                    case "cmp":
                        sb.AppendLine($"{Ind()}{DestArg(0)} = ({Arguments[1]} > 0) ? {Arguments[2]} : {Arguments[3]};");
                        break;
                    case "cnd":
                        sb.AppendLine($"{Ind()}{DestArg(0)} = ({Arguments[1]} > 0.5) ? {Arguments[2]} : {Arguments[3]};");
                        break;

                    case "crs":
                        sb.AppendLine($"{Ind()}{DestArg(0)} = cross({Arguments[1]}, {Arguments[2]});");
                        break;

                    case "dp2add":
                        sb.AppendLine($"{Ind()}{DestArg(0)} = dot({Arguments[1]}, {Arguments[2]}) + {Arguments[3]};");
                        break;

                    case "dp3":
                    case "dp4":
                        sb.AppendLine($"{Ind()}{DestArg(0)} = dot({Arguments[1]}, {Arguments[2]});");
                        break;

                    case "dsx":
                        sb.AppendLine($"{Ind()}{DestArg(0)} = dFdx({Arguments[1]});");
                        break;

                    case "dsy":
                        sb.AppendLine($"{Ind()}{DestArg(0)} = dFdy({Arguments[1]});");
                        break;

                    case "else":
                        sb.AppendLine($"{Ind()}\r\nelse {{");
                        break;

                    case "endloop":
                        sb.AppendLine("aL++;");
                        goto case "endif";

                    case "endrep":
                    case "endif":
                        context.IndentLevel--;
                        sb.AppendLine($"{Ind()}}}");
                        break;

                    case "exp":
                        sb.AppendLine($"{Ind()}{DestArg(0)} = exp({Arguments[1]}){ResolveSwizzleForDest(0, 1)};");
                        break;

                    case "frc":
                        sb.AppendLine($"{Ind()}{DestArg(0)} = fract({Arguments[1]}){ResolveSwizzleForDest(0, 1)};");
                        break;

                    case "if":
                        if (Modifier == null)
                            sb.AppendLine($"if ({Arguments[0]}) //" + Original);
                        else
                            sb.AppendLine($"if ({Arguments[0]} {comparisons[Modifier]} {Arguments[1]}) //" + Original);
                        sb.AppendLine("{");
                        context.IndentLevel++;
                        break;

                    case "label":
                        sb.AppendLine($"{Ind()}{Arguments[0]}() //" + Original);
                        sb.AppendLine("{");
                        context.IndentLevel++;
                        break;

                    case "loop":
                        sb.AppendLine($"{Ind()}while({Arguments[0]} < {Arguments[1]}) //" + Original);
                        sb.AppendLine("{");
                        context.IndentLevel++;
                        break;

                    case "log":
                        sb.AppendLine($"{Ind()}{DestArg(0)} = log({Arguments[1]});");
                        break;

                    case "lrp":
                        sb.AppendLine($"{Ind()}{DestArg(0)} = mix({Arguments[2]}, {Arguments[3]}, {Arguments[1]});");
                        break;

                    case "mul":
                        sb.AppendLine($"{Ind()}{DestArg(0)} = ({Arguments[1]} * {Arguments[2]}){ResolveSwizzleForDest(0, 1, 2)};");
                        break;

                    case "m3x2":
                    case "m3x3":
                    case "m3x4":
                    case "m4x3":
                    case "m4x4":
                        sb.AppendLine($"{Ind()}{DestArg(0)} = ({Arguments[1]} * {Arguments[2]});");
                        break;

                    case "mad":
                        sb.AppendLine($"{Ind()}{DestArg(0)} = ({Arguments[1]} * {Arguments[2]} + {Arguments[3]}){ResolveSwizzleForDest(0, 1, 2, 3)};");
                        break;

                    case "max":
                        sb.AppendLine($"{Ind()}{DestArg(0)} = max({Arguments[1]}, {Arguments[2]});");
                        break;

                    case "min":
                        sb.AppendLine($"{Ind()}{DestArg(0)} = min({Arguments[1]}, {Arguments[2]});");
                        break;

                    case "mov":
                        if (Modifier == "sat")
                            sb.AppendLine($"{Ind()}{DestArg(0)} = clamp({Arguments[1]}, 0.0, 1.0){ResolveSwizzleForDest(0, 1)};");
                        else
                            sb.AppendLine($"{Ind()}{DestArg(0)} = {Arguments[1]}{ResolveSwizzleForDest(0,1)};");
                        break;

                    case "nop":
                        break;

                    case "nrm":
                        sb.AppendLine($"{Ind()}{DestArg(0)} = normalize({Arguments[1]});");
                        break;

                    case "pow":
                        sb.AppendLine($"{Ind()}{DestArg(0)} = pow({Arguments[1]}, {Arguments[2]});");
                        break;

                    case "ps":
                        sb.AppendLine($"//" + Original);
                        break;

                    case "rcp":
                        sb.AppendLine($"{Ind()}{DestArg(0)} = (1 / {Arguments[1]}); //" + Original);
                        break;

                    case "rep":
                        var lchar = context.GetNewLoopVar();
                        sb.AppendLine($"{Ind()}for(int {lchar} = 0; {lchar} < {Arguments[0]}; {lchar}++) //" + Original);
                        sb.AppendLine("{");
                        context.IndentLevel++;
                        break;

                    case "ret":
                        sb.AppendLine($"return;");
                        context.IndentLevel--;
                        sb.AppendLine("}");
                        break;

                    case "rsq":
                        sb.AppendLine($"{Ind()}{DestArg(0)} = inversesqrt({Arguments[1]});");
                        break;

                    case "sincos":
                        var swiz = GetArgSwizzle(Arguments[0]);

                        if (swiz[0] == 'x')
                            sb.AppendLine($"{Ind()}{DestArg(0)} = cos({Arguments[1]}); //" + Original);

                        if (swiz[swiz.Length - 1] == 'y')
                            sb.AppendLine($"{Ind()}{DestArg(0)} = sin({Arguments[1]}); //" + Original);

                        break;

                    case "sub":
                        sb.AppendLine($"{Ind()}{DestArg(0)} = ({Arguments[1]} - {Arguments[2]}){ResolveSwizzleForDest(0, 1, 2)};");
                        break;

                    case "texld":
                    case "texldp":
                    case "texldb":
                        sb.AppendLine($"{Ind()}{DestArg(0)} = texture({Arguments[2]}, {Arguments[1]}{TexArgSwizzle(2)});");
                        break;

                    case "texkill":
                        sb.AppendLine($"{Ind()}if ({Arguments[0]} < 0) {{ discard; }}");
                        break;

                    case "texldd":
                        sb.AppendLine($"{Ind()}{DestArg(0)} = texldd({Arguments[1]}); //" + Original);
                        break;

                    default:
                        sb.AppendLine("//asm\r\n" + Original + "\r\n");
                        break;
                }

                var code = sb.ToString();

                return code;

                string TexArgSwizzle(int index)
                {
                    var t = context.VariableTypes[Arguments[index]];

                    switch (t)
                    {
                        case "sampler2D":
                            return ".xy";
                        default:
                            return ".xyz";
                    }
                }

                int GetLargestSwizzle(params int[] indicies)
                {
                    int max = 0;

                    foreach (var i in indicies)
                    {
                        max = Math.Max((GetArgSwizzle(Arguments[i]) ?? "xyzw").Length, max);
                    }

                    return max;
                }

                string DestArg(int index)
                {
                    var seen = context.HasBeenSeen(WithoutSwizzle(Arguments[index]));

                    if (seen == false)
                        return $"\r\n{Ind()}vec4 {WithoutSwizzle(Arguments[index])};\r\n{Ind()}{Arguments[index]}";
                    else
                        return Arguments[index];
                }

                string ResolveSwizzleForDest(int dest, params int[] args)
                {
                    var destSwizzleLength = (DestArgSwizzle(dest)?.Length ?? 1) - 1;

                    return GetLargestSwizzle(args) == destSwizzleLength ? "" : DestArgSwizzle(dest);
                }

                string DestArgSwizzle(int index)
                {
                    var sw = GetArgSwizzle(Arguments[index]);

                    if (sw != null)
                    {
                        sw = "." + sw;
                    }

                    return sw;
                }

                string Ind()
                {
                    return string.Join("", Enumerable.Repeat(" ", context.IndentLevel * context.IndentSpaces));
                }
            }
        }
    }
}
