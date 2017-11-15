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
using Reusable.MarkupBuilder.Html;
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
        
        [Parameter, Alias("np")]
        public bool NextPatch { get; set; }
        
        [Parameter, Alias("nm")]
        public bool NextMinor { get; set; }
        
        [Parameter, Alias("nr")]
        public bool NextMajor { get; set; }

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
                    Logger.ConsoleMessageLine(m => m.Prompt().span(s => s.text("Invalid version.").color(ConsoleColor.Red)));
                }
                return Task.CompletedTask;
            }

            var currentVersion = SemanticVersion.Parse(_roboNuGetFile.PackageVersion);

            if (NextPatch)
            {
                currentVersion.Patch++;
                UpdateVersion(currentVersion.ToString());
                return Task.CompletedTask;
            }

            if (NextMinor)
            {
                currentVersion.Minor++;
                currentVersion.Patch = 0;
                UpdateVersion(currentVersion.ToString());
                return Task.CompletedTask;
            }
            
            if (NextMajor)
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
            Logger.ConsoleMessageLine(m => m.Indent().text($"New version: v{_roboNuGetFile.PackageVersion}"));
        }
    }
}