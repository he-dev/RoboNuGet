using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Reusable;
using Reusable.Commander;
using Reusable.ConsoleColorizer;
using Reusable.Extensions;
using Reusable.Flawless;
using Reusable.MarkupBuilder.Html;
using Reusable.OmniLog;
using RoboNuGet.Files;

namespace RoboNuGet.Commands
{
    internal class Push : ConsoleCommand
    {
        private readonly RoboNuGetFile _roboNuGetFile;
        private readonly IFileSearch _fileSearch;
        private readonly IProcessExecutor _processExecutor;

        private static readonly IValidator<Push> ParameterValidator =
            Validator<Push>.Empty
                .IsNotValidWhen(cmd => cmd.PackageId.IsNullOrEmpty())
                .IsNotValidWhen(cmd => cmd.Version.IsNullOrEmpty());

        public Push(
            ILoggerFactory loggerFactory,
            RoboNuGetFile roboNuGetFile,
            IFileSearch fileSearch,
            IProcessExecutor processExecutor
        ) : base(loggerFactory)
        {
            _roboNuGetFile = roboNuGetFile;
            _fileSearch = fileSearch;
            _processExecutor = processExecutor;
        }

        public string PackageId { get; set; }

        public string Version { get; set; }

        public override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            //this.ValidateWith(ParameterValidator).ThrowIfNotValid();

            var solutionFileName = _fileSearch.FindSolutionFile();
            var nuspecFiles = _fileSearch.FindNuspecFiles();

            var pushStopwatch = Stopwatch.StartNew();

            var success = new Dictionary<bool, int>
            {
                [true] = 0,
                [false] = 0
            };

            // We're not pushing packages in parallel.
            foreach (var nuspecFile in nuspecFiles)
            {
                var packageStopwatch = Stopwatch.StartNew();

                var commandLine = _roboNuGetFile.NuGet.Commands["push"].Format(new
                {
                    NupkgFileName = Path.Combine(_roboNuGetFile.NuGet.OutputDirectoryName, $"{nuspecFile.Id}.{nuspecFile.Version}.nupkg"),
                    NuGetConfigName = _roboNuGetFile.NuGet.NuGetConfigName,
                });

                var result = await _processExecutor.NoWindowExecuteAsync("nuget", commandLine);
                success[result.ExitCode == ExitCode.Success]++;

                Logger.ConsoleMessageLine(m => m.text(result.Output.Trim()));
                Logger.ConsoleMessageLine(m => m.text(result.Error.Trim()));
                Logger.ConsoleMessageLine(p => p.text($"Elapsed: {packageStopwatch.Elapsed.TotalSeconds:F1} seconds"));
            }

            Logger.ConsoleMessageLine(p => p.text($"Uploaded: {success[true]} package(s)."));
            Logger.ConsoleMessageLine(p => p.text($"Failed: {success[false]} package(s)."));
            Logger.ConsoleMessageLine(p => p.text($"Elapsed: {pushStopwatch.Elapsed.TotalSeconds:F1} seconds"));
        }
    }
}