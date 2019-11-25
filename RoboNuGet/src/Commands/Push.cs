using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Reusable;
using Reusable.Commander;
using Reusable.Extensions;
using Reusable.OmniLog.Abstractions;
using t = RoboNuGet.ConsoleTemplates;
using RoboNuGet.Files;
using RoboNuGet.Services;

namespace RoboNuGet.Commands
{
    [Description("Push packages to the NuGet server.")]
    internal class Push : Command<CommandParameter>
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
            ILogger<Push> logger,
            RoboNuGetFile roboNuGetFile,
            SolutionDirectoryTree solutionDirectoryTree,
            IProcessExecutor processExecutor
        ) : base(logger)
        {
            _roboNuGetFile = roboNuGetFile;
            _solutionDirectoryTree = solutionDirectoryTree;
            _processExecutor = processExecutor;
        }


        protected override async Task ExecuteAsync(CommandParameter parameter, CancellationToken cancellationToken)
        {
            //this.ValidateWith(ParameterValidator).ThrowIfNotValid();

            var nuspecFiles = _solutionDirectoryTree.FindNuspecFiles(_roboNuGetFile.SelectedSolutionSafe().DirectoryName).ToList();

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

                var pushCommandLine = _roboNuGetFile.NuGet.Commands["push"].Format(new
                {
                    NupkgFileName = Path.Combine(_roboNuGetFile.NuGet.OutputDirectoryName, $"{nuspecFile.Id}.{nuspecFile.Version}.nupkg"),
                    NuGetConfigName = _roboNuGetFile.NuGet.NuGetConfigName,
                });

                var result = await _processExecutor.NoWindowExecuteAsync("nuget", pushCommandLine);
                success[result.ExitCode == ExitCode.Success]++;

                Logger.WriteLine(new t.NuGetCommandOutput { Text = result.Output.Trim() });
                Logger.WriteLine(new t.NuGetCommandError { Text = result.Error.Trim() });
                Logger.WriteLine(new t.NuGetCommandStopwatch { Elapsed = packageStopwatch.Elapsed });
            }

            Logger.WriteLine(new t.NuGetPushResult { TotalCount = nuspecFiles.Count, SuccessfulCount = success[true] });
            Logger.WriteLine(new t.NuGetCommandStopwatch { Elapsed = pushStopwatch.Elapsed });
        }
    }
}