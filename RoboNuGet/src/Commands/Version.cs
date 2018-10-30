using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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
    [PublicAPI]
    internal class VersionBag : SimpleBag
    {
        [Alias("r", "new")]
        [Description("Reset package version to a different one, e.g. 1.2.3")]
        public string Reset { get; set; }

        [Alias("n", "inc", "increment")]
        [Description("Increase package version by one: [major|minor|patch]")]
        public string Next { get; set; }
    }

    [Description("Change package version.")]
    [UsedImplicitly]
    [Alias("ver", "v")]
    internal class Version : ConsoleCommand<VersionBag>
    {
        private readonly RoboNuGetFile _roboNuGetFile;

        private static readonly IEnumerable<(Func<string, bool> IsNext, Func<SemanticVersion, SemanticVersion> Increment)> Updates =
            new (Func<string, bool> IsNext, Func<SemanticVersion, SemanticVersion> Increment)[]
            {
                (
                    next => SoftString.Comparer.Equals(next, nameof(SemanticVersion.Patch)),
                    current => new SemanticVersion(current.Major, current.Minor, current.Patch + 1)
                ),
                (
                    next => SoftString.Comparer.Equals(next, nameof(SemanticVersion.Minor)),
                    current => new SemanticVersion(current.Major, current.Minor + 1, 0)
                ),
                (
                    next => SoftString.Comparer.Equals(next, nameof(SemanticVersion.Major)),
                    current => new SemanticVersion(current.Major + 1, 0, 0)
                )
            };

        public Version(CommandServiceProvider<Version> serviceProvider, RoboNuGetFile roboNuGetFile) : base(serviceProvider)
        {
            _roboNuGetFile = roboNuGetFile;
        }

        protected override Task ExecuteAsync(VersionBag parameter, CancellationToken cancellationToken)
        {
            if (parameter.Reset.IsNotNull())
            {
                if (SemanticVersion.TryParse(parameter.Reset, out var version))
                {
                    UpdateVersion(version);
                }
                else
                {
                    Logger.ConsoleError("Invalid version.");
                }
            }
            else
            {
                var currentVersion = SemanticVersion.Parse(_roboNuGetFile.SelectedSolution.PackageVersion);

                foreach (var (_, increment) in Updates.Where(x => x.IsNext(parameter.Next)))
                {
                    UpdateVersion(increment(currentVersion));
                    break;
                }

                Logger.Error("Invalid arguments.");
            }

            return Task.CompletedTask;
        }

        private void UpdateVersion(SemanticVersion newVersion)
        {
            _roboNuGetFile.SelectedSolution.PackageVersion = newVersion;
            _roboNuGetFile.Save();
            Logger.WriteLine(m => m.Indent().text($"New version: v{_roboNuGetFile.SelectedSolution.PackageVersion}"));
        }
    }
}