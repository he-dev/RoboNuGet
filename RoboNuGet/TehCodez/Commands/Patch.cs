using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using JetBrains.Annotations;
using Reusable;
using Reusable.Commander;
using Reusable.ConsoleColorizer;
using Reusable.Flawless;
using Reusable.OmniLog;
using RoboNuGet.Files;

namespace RoboNuGet.Commands
{
    [UsedImplicitly]
    internal class Patch : ConsoleCommand
    {
        private readonly RoboNuGetFile _roboNuGetFile;

        public Patch(ILoggerFactory loggerFactory, RoboNuGetFile roboNuGetFile) : base(loggerFactory)
        {
            _roboNuGetFile = roboNuGetFile;
        }

        public override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var version = SemanticVersion.Parse(_roboNuGetFile.PackageVersion);
            version.Patch += 1;

            _roboNuGetFile.PackageVersion = version.ToString();
            _roboNuGetFile.Save();

            Logger.ConsoleParagraph(p => p.Indent().ConsoleText($"New version: v{_roboNuGetFile.PackageVersion}"));
            //Logger.ConsoleParagraph(p => p.ConsoleText(""));
            
            return Task.CompletedTask;
        }
    }
}