using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using JetBrains.Annotations;
using Reusable;
using Reusable.Commander;
using Reusable.Commander.Annotations;
using Reusable.Extensions;
using Reusable.MarkupBuilder.Html;
using Reusable.OmniLog;
using RoboNuGet.Files;

namespace RoboNuGet.Commands
{
    internal class VersionBag : ICommandBag
    {
        [Alias("f")]
        public string Full { get; set; }

        [Alias("np")]
        public bool NextPatch { get; set; }

        [Alias("nm")]
        public bool NextMinor { get; set; }

        [Alias("nr")]
        public bool NextMajor { get; set; }
    }

    [UsedImplicitly]
    [Alias("ver", "v")]
    internal class Version : ConsoleCommand<VersionBag>
    {
        private readonly RoboNuGetFile _roboNuGetFile;

        public Version(ILogger<Version> logger, ICommandLineMapper mapper, RoboNuGetFile roboNuGetFile) : base(logger, mapper)
        {
            _roboNuGetFile = roboNuGetFile;
        }


        protected override Task ExecuteAsync(VersionBag parameter, CancellationToken cancellationToken)
        {
            if (parameter.Full.IsNotNull())
            {
                if (SemanticVersion.TryParse(parameter.Full, out var version))
                {
                    UpdateVersion(version.ToString());
                }
                else
                {
                    Logger.WriteLine(m => m.Prompt().span(s => s.text("Invalid version.").color(ConsoleColor.Red)));
                }
                return Task.CompletedTask;
            }

            var currentVersion = SemanticVersion.Parse(_roboNuGetFile.PackageVersion);

            if (parameter.NextPatch)
            {
                //currentVersion.Patch++;
                UpdateVersion(currentVersion.ToString());
                return Task.CompletedTask;
            }

            if (parameter.NextMinor)
            {
                //currentVersion.Minor++;
                //currentVersion.Patch = 0;
                UpdateVersion(currentVersion.ToString());
                return Task.CompletedTask;
            }

            if (parameter.NextMajor)
            {
                //currentVersion.Major++;
                //currentVersion.Minor = 0;
                //currentVersion.Patch = 0;
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
            Logger.WriteLine(m => m.Indent().text($"New version: v{_roboNuGetFile.PackageVersion}"));
        }
    }
}