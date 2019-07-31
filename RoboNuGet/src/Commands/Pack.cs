using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac.Core;
using Autofac.Features.AttributeFilters;
using Autofac.Features.Indexed;
using JetBrains.Annotations;
using Reusable;
using Reusable.Collections;
using Reusable.Commander;
using Reusable.Extensions;
using Reusable.MarkupBuilder.Html;
using Reusable.OmniLog;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Console;
using t = RoboNuGet.ConsoleTemplates;
using RoboNuGet.Files;
using RoboNuGet.Services;
using SoftKeySet = Reusable.Collections.ImmutableKeySet<Reusable.SoftString>;

namespace RoboNuGet.Commands
{
    [Description("Pack packages.")]
    [UsedImplicitly]
    internal class Pack : Command<CommandLine>
    {
        private readonly RoboNuGetFile _roboNuGetFile;
        private readonly SolutionDirectoryTree _solutionDirectoryTree;
        private readonly IProcessExecutor _processExecutor;
        private readonly ICommandExecutor _commandExecutor;
        private readonly ICommandFactory _commandFactory;

        public Pack
        (
            ILogger<Pack> logger,
            RoboNuGetFile roboNuGetFile,
            SolutionDirectoryTree solutionDirectoryTree,
            IProcessExecutor processExecutor,
            ICommandExecutor commandExecutor,
            ICommandFactory commandFactory
        ) : base(logger)
        {
            _roboNuGetFile = roboNuGetFile;
            _solutionDirectoryTree = solutionDirectoryTree;
            _processExecutor = processExecutor;
            _commandExecutor = commandExecutor;
            _commandFactory = commandFactory;
        }

        protected override async Task ExecuteAsync(CommandLine commandLine, object context, CancellationToken cancellationToken)
        {
            var nuspecFiles = _solutionDirectoryTree.FindNuspecFiles(_roboNuGetFile.SelectedSolutionSafe().DirectoryName);

            var packStopwatch = Stopwatch.StartNew();

            var tasks =
                nuspecFiles
                    //.Take(5)
                    .Select(nuspecFile => Task.Run(() => CreatePackage(nuspecFile, cancellationToken), cancellationToken))
                    .ToArray();

            await Task.WhenAll(tasks).ContinueWith(t =>
            {
                var errorCount = tasks.Count(x => x.Result != ExitCode.Success);

                if (errorCount > 0)
                {
                    Logger.WriteLine(Program.Style, new t.NuGetPackResultError { ErrorCount = errorCount });
                }
                else
                {
                    Logger.WriteLine(Program.Style, new t.NuGetPackResultSuccess());
                }
            }, cancellationToken);

            Logger.WriteLine(Program.Style, new t.NuGetCommandStopwatch { Elapsed = packStopwatch.Elapsed, ThreadId = Thread.CurrentThread.ManagedThreadId });
        }

        private readonly object _consoleSyncLock = new object();

        private async Task UpdateNuspec(string packageId, string packageVersion, CancellationToken cancellationToken)
        {
            await _commandExecutor.ExecuteAsync
            (
                $"{nameof(Commands.UpdateNuspec)} " +
                $"-{nameof(UpdateNuspecCommandLine.NuspecFile)} {packageId} " +
                $"-{nameof(UpdateNuspecCommandLine.Version)} {packageVersion}",
                default(object),
                _commandFactory,
                cancellationToken
            );
        }

        private async Task<int> CreatePackage(NuspecFile nuspecFile, CancellationToken cancellationToken)
        {
            var packageStopwatch = Stopwatch.StartNew();

            var result = await Task.Run(async () =>
            {
                await UpdateNuspec(nuspecFile.Id, _roboNuGetFile.SelectedSolution.PackageVersion, cancellationToken);
                var commandLine = _roboNuGetFile.NuGet.Commands["pack"].Format(new
                {
                    NuspecFileName = nuspecFile.FileName,
                    OutputDirectoryName = _roboNuGetFile.NuGet.OutputDirectoryName,
                });
                return await _processExecutor.NoWindowExecuteAsync("nuget", commandLine);
            }, cancellationToken);

            lock (_consoleSyncLock)
            {
                //Logger.ConsoleMessageLine(m => m.text($"Executed: {result.Arguments}"));
                //Logger.WriteLine(m => m.text(result.Output.Trim()));
                Logger.WriteLine(Program.Style, new t.NuGetCommandOutput { Text = result.Output.Trim() });

                if (result.ExitCode != ExitCode.Success)
                {
                    //Logger.ConsoleError(result.Error.Trim());
                    //Logger.WriteLine(p => p.Indent().text($"Could not create package: {nuspecFile.Id}").color(ConsoleColor.Red));
                    Logger.WriteLine(Program.Style, new t.NuGetCommandError { Text = result.Error.Trim() });
                    Logger.WriteLine(Program.Style, new t.NuGetPackError { PackageId = nuspecFile.Id });
                }

                //Logger.ConsoleMessageLine(p => p.text($"Elapsed: {packageStopwatch.Elapsed.TotalSeconds:F1} sec [{Thread.CurrentThread.ManagedThreadId}] ({nuspecFile.Id})"));
                //Logger.WriteLine(p => p.text($"Elapsed: {packageStopwatch.Elapsed.TotalSeconds:F1} sec"));
                //Logger.ConsoleMessageLine(p => p.text($"-"));
                //Logger.WriteLine(_ => _);

                Logger.WriteLine(Program.Style, new t.NuGetCommandStopwatch { Elapsed = packageStopwatch.Elapsed, ThreadId = Thread.CurrentThread.ManagedThreadId });
                Logger.WriteLine(Program.Style);
            }

            return result.ExitCode;
        }
    }
}