using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Reusable.Commander;
using Reusable.Extensions;
using Reusable.Flawless;
using Reusable.OmniLog;
using RoboNuGet.Data;
using RoboNuGet.Files;

namespace RoboNuGet.Commands
{
    internal class Push : NuGet
    {
        private readonly IEnumerable<NuspecFile> _nuspecFiles;

        private static readonly Validator<Push> ParameterValidator =
            Validator<Push>.Empty
                .IsNotValidWhen(cmd => cmd.PackageId.IsNullOrEmpty())
                .IsNotValidWhen(cmd => cmd.Version.IsNullOrEmpty());

        public Push(
            ILoggerFactory loggerFactory,
            RoboNuGetFile roboNuGetFile,
            IEnumerable<NuspecFile> nuspecFiles
        ) : base(loggerFactory, roboNuGetFile)
        {
            _nuspecFiles = nuspecFiles;
            RedirectStandardOutput = true;
        }

        protected override string Name => "push";

        public string PackageId { get; set; }

        public string Version { get; set; }

        public override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            this.ValidateWith(ParameterValidator).ThrowIfNotValid();

            foreach (var nuspecFile in _nuspecFiles)
            {
                Arguments = Command.Format(new
                {
                    NupkgFileName = Path.Combine(RoboNuGetFile.NuGet.OutputDirectoryName, $"{nuspecFile.Id}.{nuspecFile.Version}.nupkg"),
                    RoboNuGetFile.NuGet.NuGetConfigName,
                });                
            }
            
            return Task.CompletedTask;
        }
    }
}