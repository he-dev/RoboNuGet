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
using Reusable.Extensions;
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

        [Parameter, Alias("f")]
        public string Full { get; set; }
        
        [Parameter, Alias("p")]
        public bool Patch { get; set; }
        
        [Parameter]
        public bool Minor { get; set; }
        
        [Parameter]
        public bool Major { get; set; }

        public override Task  ExecuteAsync(CancellationToken cancellationToken)
        {
            if (Full.IsNotNull())
            {
                if (SemanticVersion.TryParse(Full, out var version))
                {
                    UpdateVersion(version.ToString());
                }
                else
                {
                    Logger.ConsoleParagraph(p => p.Prompt().ConsoleSpan(ConsoleColor.Red, null, s => s.ConsoleText("Invalid version.")));
                }
            }

            var currentVersion = SemanticVersion.Parse(_roboNuGetFile.PackageVersion);

            if (Patch)
            {
                currentVersion.Patch++;
                UpdateVersion(currentVersion.ToString());
                return Task.CompletedTask;
            }

            if (Minor)
            {
                currentVersion.Minor++;
                currentVersion.Patch = 0;
                UpdateVersion(currentVersion.ToString());
                return Task.CompletedTask;
            }
            
            if (Major)
            {
                currentVersion.Major++;
                currentVersion.Minor = 0;
                currentVersion.Patch = 0;
                UpdateVersion(currentVersion.ToString());
                return Task.CompletedTask;
            }

            Logger.Error("Invalid arguments.");
            
            return Task.CompletedTask;
        }

        private void UpdateVersion(string newVersion)
        {
            _roboNuGetFile.PackageVersion = newVersion;
            _roboNuGetFile.Save();
            Logger.ConsoleParagraph(p => p.Indent().ConsoleText($"New version: v{_roboNuGetFile.PackageVersion}"));
        }
    }
}