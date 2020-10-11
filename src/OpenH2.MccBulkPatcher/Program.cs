using CommandLine;
using System;

namespace OpenH2.MccBulkPatcher
{
    class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<BulkPatchTaskArgs>(args)
                .WithParsed(a => BulkPatchTask.Run(a));
        }
    }
}
