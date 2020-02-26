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
        private readonly ILogger<Push> _logger;
        private readonly Session _session;
        private readonly SolutionDirectoryTree _solutionDirectoryTree;
        private readonly IProcessExecutor _processExecutor;


        public Push
        (
            ILogger<Push> logger,
            Session session,
            SolutionDirectoryTree solutionDirectoryTree,
            IProcessExecutor processExecutor
        ) 
        {
            _logger = logger;
            _session = session;
            _solutionDirectoryTree = solutionDirectoryTree;
            _processExecutor = processExecutor;
        }


        protected override async Task ExecuteAsync(CommandParameter parameter, CancellationToken cancellationToken)
        {
            //this.ValidateWith(ParameterValidator).ThrowIfNotValid();

            var nuspecFiles = _solutionDirectoryTree.FindNuspecFiles(_session.SolutionOrThrow().DirectoryName).ToList();

            var pushStopwatch = Stopwatch.StartNew();

            var success = new Dictionary<bool, int>
            {
                [true] = 0,
                [false] = 0
            };

            // We're not pushing packages in parallel.
            foreach (var nuspecFile in nuspecFiles)
            {
                var nuGet = _session.SolutionOrThrow().NuGet;
                
                var packageStopwatch = Stopwatch.StartNew();

                var pushCommandLine = nuGet.Commands["push"].Format(new
                {
                    NupkgFileName = Path.Combine(nuGet.OutputDirectoryName, $"{nuspecFile.Id}.{nuspecFile.Version}.nupkg"),
                    NuGetConfigName = nuGet.NuGetConfigName,
                });

                var result = await _processExecutor.NoWindowExecuteAsync("nuget", pushCommandLine);
                success[result.ExitCode == ExitCode.Success]++;

                _logger.WriteLine(new t.NuGetCommandOutput { Text = result.Output.Trim() });
                _logger.WriteLine(new t.NuGetCommandError { Text = result.Error.Trim() });
                _logger.WriteLine(new t.NuGetCommandStopwatch { Elapsed = packageStopwatch.Elapsed });
            }

            _logger.WriteLine(new t.NuGetPushResult { TotalCount = nuspecFiles.Count, SuccessfulCount = success[true] });
            _logger.WriteLine(new t.NuGetCommandStopwatch { Elapsed = pushStopwatch.Elapsed });
        }
    }
}