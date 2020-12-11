using CommandLine;
using OpenH2.Core.Factories;
using OpenH2.Core.Maps;
using OpenH2.Core.Maps.MCC;
using System;
using System.IO;
using System.Threading.Tasks;

namespace OpenH2.MccUtil
{
    [Verb("load")]
    public class LoadMapCommandLineArguments
    {
        [Option('f', "file", Required = true, HelpText = "The map file to load")]
        public string File { get; set; }
    }

    public class LoadMapTask
    {
        public LoadMapCommandLineArguments Args { get; }

        public static async Task Run(LoadMapCommandLineArguments args)
        {
            await new LoadMapTask(args).Run();
        }

        public LoadMapTask(LoadMapCommandLineArguments args)
        {
            this.Args = args;
        }

        public async Task Run()
        {
            var path = this.Args.File;

            if (File.Exists(path) == false)
            {
                path = Path.Combine(Environment.CurrentDirectory, path);
            }

            if (File.Exists(path) == false)
            {
                Console.WriteLine("Couldn't find file for " + this.Args.File);
                return;
            }

            var mem = new MemoryStream();
            File.OpenRead(path).CopyTo(mem);
            mem.Seek(0, SeekOrigin.Begin);
            var sig = H2BaseMap.CalculateSignature(mem);

            var factory = new MapFactory(Path.GetDirectoryName(this.Args.File));
            var h2map = factory.Load(Path.GetFileName(this.Args.File));

            if (h2map is not H2mccMap mccMap)
            {
                throw new NotSupportedException("Only MCC maps are supported in this tool");
            }

            Console.WriteLine("Loaded map");
        }
    }
}
