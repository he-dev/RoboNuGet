using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable;
using Reusable.Commander;
using Reusable.Extensions;
using Reusable.OmniLog.Abstractions;
using t = RoboNuGet.ConsoleTemplates;
using RoboNuGet.Files;
using RoboNuGet.Services;
using SoftKeySet = Reusable.Collections.ImmutableKeySet<Reusable.SoftString>;

namespace RoboNuGet.Commands
{
    [Description("Pack packages.")]
    [UsedImplicitly]
    internal class Pack : Command<CommandParameter>
    {
        private readonly Session _session;
        private readonly SolutionDirectoryTree _solutionDirectoryTree;
        private readonly IProcessExecutor _processExecutor;
        private readonly ICommandExecutor _commandExecutor;

        public Pack
        (
            ILogger<Pack> logger,
            Session session,
            SolutionDirectoryTree solutionDirectoryTree,
            IProcessExecutor processExecutor,
            ICommandExecutor commandExecutor
        ) : base(logger)
        {
            _session = session;
            _solutionDirectoryTree = solutionDirectoryTree;
            _processExecutor = processExecutor;
            _commandExecutor = commandExecutor;
        }

        protected override async Task ExecuteAsync(CommandParameter parameter, CancellationToken cancellationToken)
        {
            var nuspecFiles = _solutionDirectoryTree.FindNuspecFiles(_session.SolutionOrThrow().DirectoryName);

            var packStopwatch = Stopwatch.StartNew();

            var tasks =
                nuspecFiles
                    .Select(nuspecFile => Task.Run(() => CreatePackage(nuspecFile, cancellationToken), cancellationToken))
                    .ToArray();

            await Task.WhenAll(tasks).ContinueWith(t =>
            {
                var errorCount = tasks.Count(x => x.Result != ExitCode.Success);

                if (errorCount > 0)
                {
                    Logger.WriteLine( new t.NuGetPackResultError { ErrorCount = errorCount });
                }
                else
                {
                    Logger.WriteLine(new t.NuGetPackResultSuccess());
                }
            }, cancellationToken);

            Logger.WriteLine(new t.NuGetCommandStopwatch { Elapsed = packStopwatch.Elapsed, ThreadId = Thread.CurrentThread.ManagedThreadId });
        }

        private readonly object _consoleSyncLock = new object();

        private async Task UpdateNuspecAsync(string packageId, string packageVersion, CancellationToken cancellationToken)
        {
            await _commandExecutor.ExecuteAsync
            (
                $"{nameof(Commands.UpdateNuspec)} " +
                $"-{nameof(UpdateNuspec.Parameter.NuspecFile)} {packageId} " +
                $"-{nameof(UpdateNuspec.Parameter.Version)} {packageVersion}",
                default(object),
                cancellationToken
            );
        }

        private async Task<int> CreatePackage(NuspecFile nuspecFile, CancellationToken cancellationToken)
        {
            var packageStopwatch = Stopwatch.StartNew();

            var result = await Task.Run(async () =>
            {
                var solution = _session.SolutionOrThrow();
                await UpdateNuspecAsync(nuspecFile.Id, solution.PackageVersion, cancellationToken);
                var commandLine = solution.NuGet.Commands["pack"].Format(new
                {
                    NuspecFileName = nuspecFile.FileName,
                    OutputDirectoryName = solution.NuGet.OutputDirectoryName,
                });
                return await _processExecutor.NoWindowExecuteAsync("nuget", commandLine);
            }, cancellationToken);

            lock (_consoleSyncLock)
            {
                //Logger.ConsoleMessageLine(m => m.text($"Executed: {result.Arguments}"));
                //Logger.WriteLine(m => m.text(result.Output.Trim()));
                Logger.WriteLine(new t.NuGetCommandOutput { Text = result.Output.Trim() });

                if (result.ExitCode != ExitCode.Success)
                {
                    //Logger.ConsoleError(result.Error.Trim());
                    //Logger.WriteLine(p => p.Indent().text($"Could not create package: {nuspecFile.Id}").color(ConsoleColor.Red));
                    Logger.WriteLine(new t.NuGetCommandError { Text = result.Error.Trim() });
                    Logger.WriteLine(new t.NuGetPackError { PackageId = nuspecFile.Id });
                }

                //Logger.ConsoleMessageLine(p => p.text($"Elapsed: {packageStopwatch.Elapsed.TotalSeconds:F1} sec [{Thread.CurrentThread.ManagedThreadId}] ({nuspecFile.Id})"));
                //Logger.WriteLine(p => p.text($"Elapsed: {packageStopwatch.Elapsed.TotalSeconds:F1} sec"));
                //Logger.ConsoleMessageLine(p => p.text($"-"));
                //Logger.WriteLine(_ => _);

                Logger.WriteLine(new t.NuGetCommandStopwatch { Elapsed = packageStopwatch.Elapsed, ThreadId = Thread.CurrentThread.ManagedThreadId });
                Logger.WriteLine();
            }

            return result.ExitCode;
        }
    }
}