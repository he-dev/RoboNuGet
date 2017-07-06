using System;
using System.Collections.Generic;
using System.IO;
using Reusable.Extensions;

namespace RoboNuGet.Commands
{
    internal class PushCommand : NuGetCommand
    {
        public PushCommand(IEnumerable<string> commandLines) : base(commandLines)
        {
            RedirectStandardOutput = true;
        }

        public override string Name => "push";

        //public string PackagesDirectoryName { get; set; }

        //public string PackageId { get; set; }

        //public string Version { get; set; }

        //public string NuGetConfigFileName { get; set; }

        public override void Execute(dynamic parameter)
        {
            var nupkgFileName = $"{Path.Combine(parameter.OutputDirectory, $"{parameter.PackageId}.{parameter.FullVersion}.nupkg")}";

            var arguments = CommandLine.Format(new
            {
                NupkgFileName = nupkgFileName,
                ConfigFileName = parameter.NuGetConfigFileName,
            });

            base.Execute(arguments);            
        }

        //private string CreatePushCommand()
        //{
        //    // todo: move to config
        //    var nupkgFileName = $"{Path.Combine(PackagesDirectoryName, $"{PackageId}.{Version}.nupkg")}";
        //    return
        //        $"push " +
        //        $"\"{nupkgFileName}\" " +
        //        $"-configfile {NuGetConfigFileName}";
        //}
    }
}