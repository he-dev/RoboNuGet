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
using Reusable.Commander;
using Reusable.CommandLine;
using Reusable.ConsoleColorizer;
using Reusable.Extensions;
using Reusable.MarkupBuilder.Html;
using Reusable.OmniLog;
using RoboNuGet.Files;

namespace RoboNuGet.Commands
{
    [UsedImplicitly]
    internal class Pack : ConsoleCommand
    {
        private readonly RoboNuGetFile _roboNuGetFile;
        private readonly IFileSearch _fileSearch;
        private readonly IIndex<SoftKeySet, IConsoleCommand> _commands;
        private readonly IIsolatedFactory _isolatedFactory;

        public Pack(
            ILoggerFactory loggerFactory,
            RoboNuGetFile roboNuGetFile,
            IFileSearch fileSearch,
            IIndex<SoftKeySet, IConsoleCommand> commands,
            IIsolatedFactory isolatedFactory
        ) : base(loggerFactory)
        {
            _roboNuGetFile = roboNuGetFile;
            _fileSearch = fileSearch;
            _commands = commands;
            _isolatedFactory = isolatedFactory;
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
            var updateNuspec = (UpdateNuspec)_commands[nameof(UpdateNuspec)];
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

            var cmdExecutor = _isolatedFactory.GetIsolated<CmdExecutor>(nuspecFile.Id);

            //using (var processExecutor = new Isolated<CmdExecutor>())
            {
                var result = await Task.Run(() => cmdExecutor.Value.Execute("nuget", commandLine, CmdSwitch.EchoOff, CmdSwitch.Terminate), cancellationToken);

                lock (_consoleSyncLock)
                {
                    //Logger.ConsoleMessageLine(m => m.text($"Executed: {result.Arguments}"));
                    Logger.ConsoleMessageLine(m => m.text(result.Output.Trim()));

                    if (result.ExitCode != ExitCode.Success)
                    {
                        Logger.ConsoleError(result.Error.Trim());
                        Logger.ConsoleMessageLine(p => p.Indent().text($"Could not create package: {nuspecFile.Id}").color(ConsoleColor.Red));
                    }

                    Logger.ConsoleMessageLine(p => p.text($"Elapsed: {packageStopwatch.Elapsed.TotalSeconds:F1} sec [{Thread.CurrentThread.ManagedThreadId}] ({nuspecFile.Id})"));
                    //Logger.ConsoleMessageLine(p => p.text($"-"));
                    Logger.ConsoleMessageLine(_ => _);
                }

                return result.ExitCode;
            }
        }
    }

    public interface IIsolatedFactory : IDisposable
    {
        Isolated<T> GetIsolated<T>(string name) where T : MarshalByRefObject;
    }

    [UsedImplicitly]
    public class IsolatedFactory : IIsolatedFactory
    {
        private readonly ConcurrentDictionary<string, IDisposable> _cache = new ConcurrentDictionary<string, IDisposable>();

        public Isolated<T> GetIsolated<T>(string name) where T : MarshalByRefObject
        {
            return (Isolated<T>)_cache.GetOrAdd(name, n => new Isolated<CmdExecutor>());
        }

        public void Dispose()
        {
            foreach (var disposable in _cache)
            {
                disposable.Value.Dispose();
            }
        }
    }

    public static class CmdSwitch
    {
        public const string EchoOff = "/Q";
        public const string Terminate = "/C";
    }

    public sealed class Isolated<T> : IDisposable where T : MarshalByRefObject
    {
        private readonly AppDomain _domain;

        public Isolated()
        {
            _domain = AppDomain.CreateDomain($"Isolated-{Guid.NewGuid()}", null, AppDomain.CurrentDomain.SetupInformation);
            Value = (T)_domain.CreateInstanceAndUnwrap(typeof(T).Assembly.FullName, typeof(T).FullName);
        }

        public T Value { get; }

        public void Dispose()
        {
            AppDomain.Unload(_domain);
        }
    }

    /// <inheritdoc />
    /// <summary>
    /// Executes commands in a new instance of the Windows command interpreter.
    /// </summary>
    [UsedImplicitly]
    public class CmdExecutor : MarshalByRefObject
    {
        public CmdResult Execute([NotNull] string fileName, [NotNull] string arguments, params string[] cmdSwitches)
        {
            if (cmdSwitches == null) throw new ArgumentNullException(nameof(cmdSwitches));
            if (fileName == null) throw new ArgumentNullException(nameof(fileName));
            if (arguments == null) throw new ArgumentNullException(nameof(arguments));

            arguments = $"{cmdSwitches.Join(" ")} {fileName} {arguments}";
            var startInfo = new ProcessStartInfo("cmd", arguments)
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            };

            using (var process = new Process { StartInfo = startInfo })
            {
                var output = new StringBuilder();
                var error = new StringBuilder();

                process.OutputDataReceived += (sender, e) =>
                {
                    output.AppendLine(e.Data);
                };

                process.ErrorDataReceived += (sender, e) =>
                {
                    error.AppendLine(e.Data);
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();

                return new CmdResult
                {
                    Arguments = arguments,
                    Output = output.ToString(),
                    Error = error.ToString(),
                    ExitCode = process.ExitCode
                };
            }
        }
    }

    [Serializable]
    public class CmdResult
    {
        public string Arguments { get; set; }
        public string Output { get; set; }
        public string Error { get; set; }
        public int ExitCode { get; set; }
    }
}