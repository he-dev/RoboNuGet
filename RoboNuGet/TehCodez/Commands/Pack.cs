using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Reusable.Extensions;
using Reusable.OmniLog;
using RoboNuGet.Data;

namespace RoboNuGet.Commands
{
    internal class Pack : NuGet
    {
        public Pack(ILoggerFactory loggerFactory, RoboNuGetFile roboNuGetFile) : base(loggerFactory, roboNuGetFile)
        {
            RedirectStandardOutput = true;
        }

        protected override string Name => "pack";

        public string NuspecFileName { get; set; }

        public string OutputDirectory { get; set; }

        public override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            Arguments = Command.Format(new
            {
                NuspecFileName,
                OutputDirectory,
            });

            return base.ExecuteAsync(cancellationToken);
        }        
    }
}