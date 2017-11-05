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

namespace RoboNuGet.Commands
{
    internal class Push : NuGet
    {
        private static readonly Validator<Push> ParameterValidator =
            Validator<Push>.Empty
                .IsNotValidWhen(cmd => cmd.PackageId.IsNullOrEmpty())
                .IsNotValidWhen(cmd => cmd.Version.IsNullOrEmpty());

        public Push(ILoggerFactory loggerFactory, RoboNuGetFile roboNuGetFile) : base(loggerFactory, roboNuGetFile)
        {
            RedirectStandardOutput = true;
        }

        protected override string Name => "push";

        public string PackageId { get; set; }

        public string Version { get; set; }

        public override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            this.ValidateWith(ParameterValidator).ThrowIfNotValid();
            
            // ReSharper disable once InconsistentNaming - we make an excepiton to avoid additional properties.
            var NupkgFileName = Path.Combine(
                RoboNuGetFile.NuGet.OutputDirectoryName,
                $"{PackageId}.{Version}.nupkg");

            Arguments = Command.Format(new
            {
                NupkgFileName,
                RoboNuGetFile.NuGet.NuGetConfigName,
            });

            return base.ExecuteAsync(cancellationToken);
        }
    }
}