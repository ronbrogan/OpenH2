using Microsoft.Build.Utilities;
using OpenH2.Core.Tags;
using OpenH2.Core.Tags.Layout;
using OpenH2.Core.Tags.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Resources;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace OpenH2.Core.CodeGenTests
{
    public class TagCreatorGeneratorTests : XunitLoggingBase
    {
        private string AssemblyName = "TagSerializer";

        private readonly ITestOutputHelper output;

        public TagCreatorGeneratorTests(ITestOutputHelper output) : base(output)
        {
            this.output = output;
        }

        [Fact]
        public void TestGeneratedAssembly()
        {
            var tagTypes = typeof(BaseTag).Assembly.GetTypes()
                .Where(t => typeof(BaseTag).IsAssignableFrom(t) && t.IsAbstract == false)
                .Select(t => new { Type = t, Attribute = t.GetCustomAttribute<TagLabelAttribute>() })
                .Where(x => x.Attribute != null)
                .Select(x => x.Type);

            var assy = GenerateAssembly(tagTypes);

            var (code, output) = ExecutePEVerify(assy);

            this.output.WriteLine("================= Start PEVerify Output =================");
            this.output.WriteLine(output);
            this.output.WriteLine("================= End PEVerify Output =================");

            Assert.Equal(0, code);
        }

        internal (int,string) ExecutePEVerify(string assyPath)
        {
            var peverify = ToolLocationHelper.GetPathToDotNetFrameworkSdkFile("PEVerify.exe");

            var writer = new StringWriter();

            var pStart = new ProcessStartInfo(peverify, assyPath);
            pStart.RedirectStandardOutput = true;
            pStart.UseShellExecute = false;

            using (var proc = Process.Start(pStart))
            {
                proc.OutputDataReceived += (s, e) => writer.WriteLine(e.Data);
                proc.BeginOutputReadLine();


                proc.WaitForExit(10000);

                if (proc.HasExited)
                {
                    return (proc.ExitCode, writer.GetStringBuilder().ToString());
                }
                else
                {
                    return (-1, writer.GetStringBuilder().ToString() + "\r\n PEVerify timed out after 10s");
                }
            } 
        }

        internal string GenerateAssembly(IEnumerable<Type> types)
        {
            var assyName = new AssemblyName(AssemblyName);

            var dom = Thread.GetDomain();

            var assembly = dom.DefineDynamicAssembly(assyName,
                AssemblyBuilderAccess.RunAndSave);

            var module = assembly.DefineDynamicModule(AssemblyName, AssemblyName + ".dll", true);

            var type = module.DefineType(AssemblyName, TypeAttributes.Public);

            TagCreatorGenerator.builderWrapperFactory = tagType =>
            {
                var builder = type.DefineMethod("Read" + tagType.Name,
                    MethodAttributes.Public | MethodAttributes.Static,
                    typeof(object),
                    TagCreatorGenerator.arguments);

                var wrapper = new TagCreatorGenerator.MethodBuilderWrapper()
                {
                    generator = builder.GetILGenerator(),
                    getDelegate = () => {
                        // dummy delegate for type gen purposes, can't use method until type is created
                        var delg = new TagCreator((s, i, j) => null);

                        return delg;
                    }
                };

                return wrapper;
            };

            foreach (var t in types)
            {
                TagCreatorGenerator.GetTagCreator(t);
            }

            type.CreateType();

            var outputFile = assyName + ".dll";

            assembly.Save(outputFile);

            return Path.Combine(dom.SetupInformation.ApplicationBase, outputFile);
        }
    }
}
