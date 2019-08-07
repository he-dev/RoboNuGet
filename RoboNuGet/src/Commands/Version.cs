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
using Reusable.Data.Annotations;
using Reusable.Extensions;
using Reusable.MarkupBuilder.Html;
using Reusable.OmniLog;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Extensions;
using t = RoboNuGet.ConsoleTemplates;
using RoboNuGet.Files;

namespace RoboNuGet.Commands
{
    [PublicAPI]
    internal class VersionCommandLine : CommandLine
    {
        public VersionCommandLine(CommandLineDictionary arguments) : base(arguments) { }

        [Tags("s")]
        [Description("Set package version to a different one, e.g. 1.2.3")]
        public string Set => GetArgument(() => Set);

        [Tags("n", "inc", "increment")]
        [Description("Increment package version by one: [major|minor|patch]")]
        public string Next => GetArgument(() => Next);
    }

    [Description("Change package version.")]
    [UsedImplicitly]
    [Tags("ver", "v")]
    internal class Version : Command<VersionCommandLine>
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

        public Version(ILogger<Version> logger, RoboNuGetFile roboNuGetFile) : base(logger)
        {
            _roboNuGetFile = roboNuGetFile;
        }

        protected override Task ExecuteAsync(VersionCommandLine commandLine, object context, CancellationToken cancellationToken)
        {
            _roboNuGetFile.SelectedSolutionSafe();

            if (commandLine.Set.IsNotNull())
            {
                if (SemanticVersion.TryParse(commandLine.Set, out var version))
                {
                    UpdateVersion(version);
                }
                else
                {
                    Logger.WriteLine(new t.Error { Text = "Invalid version" });
                }
            }
            else
            {
                var currentVersion = SemanticVersion.Parse(_roboNuGetFile.SelectedSolution.PackageVersion);

                var next = commandLine.Next;
                foreach (var (_, increment) in Updates.Where(x => x.IsNext(next)))
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
            Logger.WriteLine(new t.Version.Response { NewVersion = _roboNuGetFile.SelectedSolution.PackageVersion });
        }
    }
}