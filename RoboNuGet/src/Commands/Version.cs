using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable;
using Reusable.Commander;
using Reusable.Data.Annotations;
using Reusable.Extensions;
using Reusable.OmniLog;
using Reusable.OmniLog.Abstractions;
using t = RoboNuGet.ConsoleTemplates;
using RoboNuGet.Files;

namespace RoboNuGet.Commands
{
    [Description("Change package version.")]
    [UsedImplicitly]
    [Alias("ver", "v")]
    internal class Version : Command<Version.Parameter>
    {
        private readonly Session _session;

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

        public Version(ILogger<Version> logger, Session session) : base(logger)
        {
            _session = session;
        }

        protected override Task ExecuteAsync(Parameter parameter, CancellationToken cancellationToken)
        {
            var solution = _session.SolutionOrThrow();

            if (parameter.Set is {})
            {
                if (SemanticVersion.TryParse(parameter.Set, out var version))
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
                if (parameter.Next is {})
                {
                    var currentVersion = SemanticVersion.Parse(solution.PackageVersion);
                    foreach (var (_, increment) in Updates.Where(x => x.IsNext(parameter.Next)))
                    {
                        UpdateVersion(increment(currentVersion));
                        break;
                    }
                }
                else
                {
                    Logger.Error("Invalid arguments.");
                }
            }

            return Task.CompletedTask;
        }

        private void UpdateVersion(SemanticVersion newVersion)
        {
            _session.SolutionOrThrow().PackageVersion = newVersion;
            _session.Config.Save();
            Logger.WriteLine(new t.Version.Response { NewVersion = newVersion });
        }

        [PublicAPI]
        internal class Parameter : CommandParameter
        {
            [Alias("s")]
            [Description("Set package version to a different one, e.g. 1.2.3")]
            public string? Set { get; set; }

            [Alias("n", "inc", "increment")]
            [Description("Increment package version by one: [major|minor|patch]")]
            public string? Next { get; set; }
        }
    }
}