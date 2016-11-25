using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RoboNuGet.Commands
{
    internal abstract class NuGetCommand : StartProcessCommand
    {
        protected NuGetCommand(IEnumerable<string> commandsLines)
        {
            CommandLine = commandsLines.Single(x => x.StartsWith(Name, StringComparison.OrdinalIgnoreCase));
            RedirectStandardOutput = true;
        }

        public abstract string Name { get; }

        public string CommandLine { get; }

        public override void Execute(object parameter)
        {
            base.Execute(new
            {
                FileName = "nuget",
                Arguments = (string)parameter
            });
        }
    }
}