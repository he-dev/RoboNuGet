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
        private readonly IFileSearch _fileSearch;

        private static readonly Validator<Push> ParameterValidator =
            Validator<Push>.Empty
                .IsNotValidWhen(cmd => cmd.PackageId.IsNullOrEmpty())
                .IsNotValidWhen(cmd => cmd.Version.IsNullOrEmpty());

        public Push(
            ILoggerFactory loggerFactory,
            RoboNuGetFile roboNuGetFile,
            IFileSearch fileSearch
        ) : base(loggerFactory)
        {
            _fileSearch = fileSearch;
            RedirectStandardOutput = true;
        }

        //protected override string Name => "push";

        public string PackageId { get; set; }

        public string Version { get; set; }

        public override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            this.ValidateWith(ParameterValidator).ThrowIfNotValid();

            var solutionFileName = _fileSearch.FindSolutionFile();
            var nuspecFiles = _fileSearch.FindNuspecFiles();
            
            // We're not pushing packages in parallel.
            foreach (var nuspecFile in nuspecFiles)
            {
                //Arguments = Command.Format(new
                //{
                //    NupkgFileName = Path.Combine(RoboNuGetFile.NuGet.OutputDirectoryName, $"{nuspecFile.Id}.{nuspecFile.Version}.nupkg"),
                //    RoboNuGetFile.NuGet.NuGetConfigName,
                //});                
            }
            
            return Task.CompletedTask;
        }
    }
}