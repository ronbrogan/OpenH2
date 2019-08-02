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
using System.Threading;
using Xunit;
using Xunit.Abstractions;

namespace OpenH2.Core.CodeGenTests
{
    public class TagCreatorGeneratorTests : XunitLoggingBase
    {
        private readonly ITestOutputHelper output;

        public TagCreatorGeneratorTests(ITestOutputHelper output) : base(output)
        {
            this.output = output;
        }

        [Fact]
        public void AllTags_PassPEVerify()
        {
            var tagTypes = typeof(BaseTag).Assembly.GetTypes()
                .Where(t => typeof(BaseTag).IsAssignableFrom(t) && t.IsAbstract == false)
                .Select(t => new { Type = t, Attribute = t.GetCustomAttribute<TagLabelAttribute>() })
                .Where(x => x.Attribute != null)
                .Select(x => x.Type);

            var assy = GenerateAssembly("TagSerializer", tagTypes);

            var code = ExecutePEVerify(assy);

            Assert.Equal(0, code);
        }

        [Fact]
        public void TestTag_PassesPEVerify()
        {
            var assy = GenerateAssembly("TestTagSerializer", new[] { typeof(TestTag), typeof(TestTag) });

            var code = ExecutePEVerify(assy);

            Assert.Equal(0, code);
        }

        internal int ExecutePEVerify(string assyPath)
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

                this.output.WriteLine("================= Start PEVerify Output =================");
                this.output.WriteLine(writer.GetStringBuilder().ToString());
                this.output.WriteLine("================= End PEVerify Output =================");

                if (proc.HasExited)
                {
                    return proc.ExitCode;
                }
                else
                {
                    return -1;
                }
            } 
        }

        internal string GenerateAssembly(string assemblyName, IEnumerable<Type> types)
        {
            var assyName = new AssemblyName(assemblyName);

            var dom = Thread.GetDomain();

            var assembly = dom.DefineDynamicAssembly(assyName,
                AssemblyBuilderAccess.RunAndSave);

            var module = assembly.DefineDynamicModule(assemblyName, assemblyName + ".dll", true);

            var generator = new TagCreatorGenerator(module);

            foreach (var t in types)
            {
                generator.GetTagCreator(t);
            }

            var outputFile = assyName + ".dll";

            assembly.Save(outputFile);

            return Path.Combine(dom.SetupInformation.ApplicationBase, outputFile);
        }

        public class TestTag : BaseTag
        {
            public TestTag(uint id) : base(id)
            {
            }

            [PrimitiveValue(0)]
            public int Value1 { get; set; }

            [PrimitiveValue(4)]
            public float Value2 { get; set; }

            [InternalReferenceValue(8)]
            public SubTag[] SubValues { get; set; }
            public override string Name { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            [FixedLength(12)]
            public struct SubTag
            {
                [PrimitiveValue(0)]
                public int Deadbeef { get; set; }

                [InternalReferenceValue(4)]
                public SubSubTag[] SubSubTags { get; set; }

                [FixedLength(4)]
                public struct SubSubTag
                {
                    [PrimitiveValue(0)]
                    public float Value { get; set; }
                }
            }
        }
    }
}
