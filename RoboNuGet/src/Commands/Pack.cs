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
using RoboNuGet.Files;
using RoboNuGet.Services;
using SoftKeySet = Reusable.Collections.ImmutableKeySet<Reusable.SoftString>;

namespace RoboNuGet.Commands
{
    [Description("Pack packages.")]
    [UsedImplicitly]
    internal class Pack : Command
    {
        private readonly RoboNuGetFile _roboNuGetFile;
        private readonly SolutionDirectoryTree _solutionDirectoryTree;
        private readonly IProcessExecutor _processExecutor;

        public Pack
        (
            CommandServiceProvider<Pack> serviceProvider,
            RoboNuGetFile roboNuGetFile,
            SolutionDirectoryTree solutionDirectoryTree,
            IProcessExecutor processExecutor
        ) : base(serviceProvider, nameof(Pack))
        {
            _roboNuGetFile = roboNuGetFile;
            _solutionDirectoryTree = solutionDirectoryTree;
            _processExecutor = processExecutor;
        }

        protected override async Task ExecuteAsync(ICommandLineReader<ICommandArgumentGroup> parameter, object context, CancellationToken cancellationToken)
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
                    Logger.ConsoleError($"{tasks.Count(x => x.Result != ExitCode.Success)} error(s) occured.");
                }
                else
                {
                    Logger.WriteLine(p => p.Indent().span(s => s.text("All packages successfully created.").color(ConsoleColor.Green)));
                }
            }, cancellationToken);

            Logger.WriteLine(p => p.Indent().text($"Elapsed: {packStopwatch.Elapsed.TotalSeconds:F1} sec [{Thread.CurrentThread.ManagedThreadId}]"));
        }

        private readonly object _consoleSyncLock = new object();

        private async Task UpdateNuspec(string packageId, string packageVersion, CancellationToken cancellationToken)
        {
            await Services.Executor.ExecuteAsync
            (
                $"{nameof(Commands.UpdateNuspec)} " +
                $"-{nameof(IUpdateNuspecParameter.NuspecFile)} {packageId} " +
                $"-{nameof(IUpdateNuspecParameter.Version)} {packageVersion}",
                default(object),
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
                Logger.WriteLine(m => m.text(result.Output.Trim()));

                if (result.ExitCode != ExitCode.Success)
                {
                    Logger.ConsoleError(result.Error.Trim());
                    Logger.WriteLine(p => p.Indent().text($"Could not create package: {nuspecFile.Id}").color(ConsoleColor.Red));
                }

                //Logger.ConsoleMessageLine(p => p.text($"Elapsed: {packageStopwatch.Elapsed.TotalSeconds:F1} sec [{Thread.CurrentThread.ManagedThreadId}] ({nuspecFile.Id})"));
                Logger.WriteLine(p => p.text($"Elapsed: {packageStopwatch.Elapsed.TotalSeconds:F1} sec"));
                //Logger.ConsoleMessageLine(p => p.text($"-"));
                Logger.WriteLine(_ => _);
            }

            return result.ExitCode;
        }
    }
}