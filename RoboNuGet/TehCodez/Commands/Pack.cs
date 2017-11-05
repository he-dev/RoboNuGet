using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Autofac.Features.Indexed;
using Reusable.Commander;
using Reusable.CommandLine;
using Reusable.Extensions;
using Reusable.OmniLog;
using RoboNuGet.Data;
using RoboNuGet.Files;

namespace RoboNuGet.Commands
{
    internal class Pack : NuGet
    {
        private readonly IEnumerable<NuspecFile> _nuspecFiles;
        private readonly IIndex<SoftKeySet, IConsoleCommand> _commands;

        public Pack(
            ILoggerFactory loggerFactory,
            RoboNuGetFile roboNuGetFile,
            IEnumerable<NuspecFile> nuspecFiles,
            IIndex<SoftKeySet,
                IConsoleCommand> commands
        ) : base(loggerFactory, roboNuGetFile)
        {
            _nuspecFiles = nuspecFiles;
            _commands = commands;
            RedirectStandardOutput = true;
        }

        protected override string Name => "pack";

        public string NuspecFileName { get; set; }

        public string OutputDirectory { get; set; }

        public override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            try
            {
                Parallel.ForEach(_nuspecFiles, async nuspecFile =>
                {
                    var updateNuspec = (UpdateNuspec) _commands[nameof(UpdateNuspec)];
                    updateNuspec.NuspecFile = nuspecFile;
                    updateNuspec.Version = RoboNuGetFile.PackageVersion;
                    await updateNuspec.ExecuteAsync(cancellationToken);

                    Arguments = Command.Format(new
                    {
                        NuspecFileName,
                        OutputDirectory,
                    });

                    await base.ExecuteAsync(cancellationToken);
                });

                //ConsoleColorizer.RenderLine($"<p>&gt;<span color='green'>All packages successfuly created.</span> <span color='darkyellow'>(Press Enter to continue)</span></p>");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                //ConsoleColorizer.RenderLine($"<p>&gt;<span color='green'>Some packages could not be created.</span> <span color='darkyellow'>(Press Enter to continue)</span></p>");
                Picasso.WriteError(ex.Message);
            }
        }
    }
}