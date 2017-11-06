using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using JetBrains.Annotations;
using Reusable;
using Reusable.Commander;
using Reusable.CommandLine;
using Reusable.ConsoleColorizer;
using Reusable.OmniLog;
using RoboNuGet.Files;

namespace RoboNuGet.Commands
{
    [UsedImplicitly]
    [Alias("ver", "v")]
    internal class Version : ConsoleCommand
    {
        private readonly RoboNuGetFile _roboNuGetFile;

        public Version(ILoggerFactory loggerFactory, RoboNuGetFile roboNuGetFile) : base(loggerFactory)
        {
            _roboNuGetFile = roboNuGetFile;
        }

        [Parameter(Position = 1)]
        public string NewVersion { get; set; }

        public override Task  ExecuteAsync(CancellationToken cancellationToken)
        {
            if (SemanticVersion.TryParse(NewVersion, out var version))
            {
                _roboNuGetFile.PackageVersion = version.ToString();
                _roboNuGetFile.Save();
                
                Logger.ConsoleParagraph(p => p.Prompt().ConsoleText("Version updated."));
            }
            else
            {
                Logger.ConsoleParagraph(p => p.Prompt().ConsoleSpan(ConsoleColor.Red, null, s => s.ConsoleText("Invalid version.")));
            }

            return Task.CompletedTask;
        }
    }
}