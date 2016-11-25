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

        public override void Execute(dynamic parameter)
        {
            var arguments = CommandLine.Format(new
            {
                FileName = parameter.PackageNuspec.FileName,
                OutputDirectory = parameter.OutputDirectory,
            });

            base.Execute(arguments);
        }        
    }
}