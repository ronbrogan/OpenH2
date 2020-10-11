using CommandLine;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenH2.MccUtil
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await Parser.Default
                .ParseArguments<
                    UnpackCommandLineArguments,
                    PackCommandLineArguments,
                    LoadMapCommandLineArguments,
                    DumpScriptsCommandLineArguments,
                    PatchScriptCommandLineArguments
                    >(args)
                .MapResult(
                    async (UnpackCommandLineArguments a) => await UnpackTask.Run(a),
                    async (PackCommandLineArguments a) => await PackTask.Run(a),
                    async (LoadMapCommandLineArguments a) => await LoadMapTask.Run(a),
                    async (DumpScriptsCommandLineArguments a) => await DumpScriptsTask.Run(a),
                    async (PatchScriptCommandLineArguments a) => await PatchScriptTask.Run(a),
                    errs => WriteErrors(errs));
        }

        static async Task WriteErrors(IEnumerable<Error> errs)
        {
            foreach (var err in errs)
                Console.WriteLine(err.ToString());
        }
    }
}
