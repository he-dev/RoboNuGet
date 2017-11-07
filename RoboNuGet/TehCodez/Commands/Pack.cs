using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Autofac.Features.Indexed;
using JetBrains.Annotations;
using Reusable.Commander;
using Reusable.CommandLine;
using Reusable.ConsoleColorizer;
using Reusable.Extensions;
using Reusable.OmniLog;
using RoboNuGet.Files;

namespace RoboNuGet.Commands
{
    [UsedImplicitly]
    internal class Pack : NuGet
    {
        private readonly IFileService _fileService;
        private readonly IIndex<SoftKeySet, IConsoleCommand> _commands;

        public Pack(
            ILoggerFactory loggerFactory,
            RoboNuGetFile roboNuGetFile,
            IFileService fileService,
            IIndex<SoftKeySet, IConsoleCommand> commands
        ) : base(loggerFactory, roboNuGetFile)
        {
            _fileService = fileService;
            _commands = commands;
            RedirectStandardOutput = true;
        }

        protected override string Name => "pack";

        //public string NuspecFileName { get; set; }

        //public string OutputDirectory { get; set; }

        public override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var solutionFileName = _fileService.GetSolutionFileName(RoboNuGetFile.SolutionFileName);
            var nuspecFiles = _fileService.GetNuspecFiles(Path.GetDirectoryName(solutionFileName));

            var errorCount = 0;

            Parallel.ForEach(nuspecFiles, async nuspecFile =>
            {
                var updateNuspec = (UpdateNuspec) _commands[nameof(UpdateNuspec)];
                updateNuspec.NuspecFile = nuspecFile;
                updateNuspec.Version = RoboNuGetFile.PackageVersion;
                await updateNuspec.ExecuteAsync(cancellationToken);

                Arguments = Command.Format(new
                {
                    NuspecFileName = nuspecFile.FileName,
                    OutputDirectoryName = RoboNuGetFile.NuGet.OutputDirectoryName,
                });

                try
                {
                    await base.ExecuteAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    errorCount++;
                    Logger.ConsoleParagraph(p => p.Indent().ConsoleText($"Could not create package: {nuspecFile.Id}"));
                    Logger.ConsoleException(ex);
                }
            });

            if (errorCount == 0)
            {
                Logger.ConsoleParagraph(p => p.Indent().ConsoleSpan(ConsoleColor.Green, null, s => s.ConsoleText("All packages successfuly created.")));
            }

            return Task.CompletedTask;
        }
    }
}