using System;
using System.Collections.Generic;
using System.IO;
using Reusable;

namespace RoboNuGet.Commands
{
    internal class PackCommand : NuGetCommand
    {
        public PackCommand(IEnumerable<string> commandLines) : base(commandLines)
        {
            RedirectStandardOutput = true;
        }

        public override string Name => "pack";

        //public PackageNuspec PackageNuspec { get; set; }

        //public string Version { get; set; }

        //public string Outputdirectory { get; set; }

        public override void Execute(dynamic parameter)
        {
            var arguments = CommandLine.Format(new
            {
                FileName = parameter.FileName,
                OutputDirectory = parameter.OutputDirectory,
            });

            base.Execute(arguments);
        }        
    }
}