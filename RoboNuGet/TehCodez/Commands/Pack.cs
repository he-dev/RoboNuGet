using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac.Features.Indexed;
using JetBrains.Annotations;
using Reusable;
using Reusable.Collections;
using Reusable.Commander;
using Reusable.ConsoleColorizer;
using Reusable.Extensions;
using Reusable.MarkupBuilder.Html;
using Reusable.OmniLog;
using RoboNuGet.Files;
using SoftKeySet = Reusable.Collections.ImmutableKeySet<Reusable.SoftString>;

namespace RoboNuGet.Commands
{
    [UsedImplicitly]
    internal class Pack : ConsoleCommand
    {
        private readonly RoboNuGetFile _roboNuGetFile;
        private readonly IFileSearch _fileSearch;
        private readonly IIndex<SoftKeySet, IConsoleCommand> _commands;
        private readonly IProcessExecutor _processExecutor;

        public Pack(
            ILoggerFactory loggerFactory,
            RoboNuGetFile roboNuGetFile,
            IFileSearch fileSearch,
            IIndex<SoftKeySet, IConsoleCommand> commands,
            IProcessExecutor processExecutor
        ) : base(loggerFactory)
        {
            _roboNuGetFile = roboNuGetFile;
            _fileSearch = fileSearch;
            _commands = commands;
            _processExecutor = processExecutor;
        }

        public override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var nuspecFiles = _fileSearch.FindNuspecFiles();

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
                    Logger.ConsoleMessageLine(p => p.Indent().span(s => s.text("All packages successfuly created.").color(ConsoleColor.Green)));
                }

            }, cancellationToken);

            Logger.ConsoleMessageLine(p => p.Indent().text($"Elapsed: {packStopwatch.Elapsed.TotalSeconds:F1} sec [{Thread.CurrentThread.ManagedThreadId}]"));
        }

        private readonly object _consoleSyncLock = new object();

        private async Task UpdateNuspec(NuspecFile nuspecFile, CancellationToken cancellationToken)
        {
            var updateNuspec = (UpdateNuspec)_commands[ImmutableKeySet<SoftString>.Create(nameof(UpdateNuspec))];
            updateNuspec.NuspecFile = nuspecFile;
            updateNuspec.Version = _roboNuGetFile.PackageVersion;

            await updateNuspec.ExecuteAsync(cancellationToken);
        }

        private async Task<int> CreatePackage(NuspecFile nuspecFile, CancellationToken cancellationToken)
        {
            var packageStopwatch = Stopwatch.StartNew();

            await UpdateNuspec(nuspecFile, cancellationToken);

            var commandLine = _roboNuGetFile.NuGet.Commands["pack"].Format(new
            {
                NuspecFileName = nuspecFile.FileName,
                OutputDirectoryName = _roboNuGetFile.NuGet.OutputDirectoryName,
            });

            var result = await Task.Run(() => _processExecutor.NoWindowExecuteAsync("nuget", commandLine), cancellationToken);

            lock (_consoleSyncLock)
            {
                //Logger.ConsoleMessageLine(m => m.text($"Executed: {result.Arguments}"));
                Logger.ConsoleMessageLine(m => m.text(result.Output.Trim()));

                if (result.ExitCode != ExitCode.Success)
                {
                    Logger.ConsoleError(result.Error.Trim());
                    Logger.ConsoleMessageLine(p => p.Indent().text($"Could not create package: {nuspecFile.Id}").color(ConsoleColor.Red));
                }

                //Logger.ConsoleMessageLine(p => p.text($"Elapsed: {packageStopwatch.Elapsed.TotalSeconds:F1} sec [{Thread.CurrentThread.ManagedThreadId}] ({nuspecFile.Id})"));
                Logger.ConsoleMessageLine(p => p.text($"Elapsed: {packageStopwatch.Elapsed.TotalSeconds:F1} sec"));
                //Logger.ConsoleMessageLine(p => p.text($"-"));
                Logger.ConsoleMessageLine(_ => _);
            }

            return result.ExitCode;
        }
    }
}