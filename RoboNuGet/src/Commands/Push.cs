using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Reusable;
using Reusable.Commander;
using Reusable.Commander.Services;
using Reusable.Extensions;
using Reusable.MarkupBuilder.Html;
using Reusable.OmniLog;
using RoboNuGet.Files;
using RoboNuGet.Services;

namespace RoboNuGet.Commands
{
    internal class PushBag : SimpleBag
    {
        public string PackageId { get; set; }

        public string Version { get; set; }
    }

    [Description("Push packages to the NuGet server.")]
    internal class Push : ConsoleCommand
    {
        private readonly RoboNuGetFile _roboNuGetFile;
        private readonly SolutionDirectoryTree _solutionDirectoryTree;
        private readonly IProcessExecutor _processExecutor;

        //private static readonly IValidator<Push> ParameterValidator =
        //    Validator<Push>.Empty
        //        .IsNotValidWhen(cmd => cmd.PackageId.IsNullOrEmpty())
        //        .IsNotValidWhen(cmd => cmd.Version.IsNullOrEmpty());

        public Push
        (
            CommandServiceProvider<Push> serviceProvider,
            RoboNuGetFile roboNuGetFile,
            SolutionDirectoryTree solutionDirectoryTree,
            IProcessExecutor processExecutor
        ) : base(serviceProvider, nameof(Push))
        {
            _roboNuGetFile = roboNuGetFile;
            _solutionDirectoryTree = solutionDirectoryTree;            
            _processExecutor = processExecutor;
        }


        protected override async Task ExecuteAsync(ICommandLineReader<ICommandParameter> parameter, NullContext context, CancellationToken cancellationToken)
        {
            //this.ValidateWith(ParameterValidator).ThrowIfNotValid();

            var nuspecFiles = _solutionDirectoryTree.FindNuspecFiles(_roboNuGetFile.SelectedSolutionSafe().DirectoryName);

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

                Logger.WriteLine(m => m.text(result.Output.Trim()));
                Logger.WriteLine(m => m.text(result.Error.Trim()));
                Logger.WriteLine(p => p.text($"Elapsed: {packageStopwatch.Elapsed.TotalSeconds:F1} seconds"));
            }

            Logger.WriteLine(p => p.text($"Uploaded: {success[true]} package(s)."));
            Logger.WriteLine(p => p.text($"Failed: {success[false]} package(s)."));
            Logger.WriteLine(p => p.text($"Elapsed: {pushStopwatch.Elapsed.TotalSeconds:F1} seconds"));
        }
    }
}