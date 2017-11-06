using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Reusable.Commander;
using Reusable.Extensions;
using Reusable.Flawless;
using Reusable.OmniLog;
using RoboNuGet.Files;

namespace RoboNuGet.Commands
{
    internal class Push : NuGet
    {
        private readonly IFileService _fileService;

        private static readonly Validator<Push> ParameterValidator =
            Validator<Push>.Empty
                .IsNotValidWhen(cmd => cmd.PackageId.IsNullOrEmpty())
                .IsNotValidWhen(cmd => cmd.Version.IsNullOrEmpty());

        public Push(
            ILoggerFactory loggerFactory,
            RoboNuGetFile roboNuGetFile,
            IFileService fileService
        ) : base(loggerFactory, roboNuGetFile)
        {
            _fileService = fileService;
            RedirectStandardOutput = true;
        }

        protected override string Name => "push";

        public string PackageId { get; set; }

        public string Version { get; set; }

        public override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            this.ValidateWith(ParameterValidator).ThrowIfNotValid();

            var solutionFileName = _fileService.GetSolutionFileName(RoboNuGetFile.SolutionFileName);
            var nuspecFiles = _fileService.GetNuspecFiles(Path.GetDirectoryName(solutionFileName));
            
            // We're not pushing packages in parallel.
            foreach (var nuspecFile in nuspecFiles)
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