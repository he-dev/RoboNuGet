using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.OmniLog;
using RoboNuGet.Files;

namespace RoboNuGet.Commands
{
    [UsedImplicitly]
    internal class NuGet : StartProcess
    {
        public NuGet(ILoggerFactory loggerFactory) : base(loggerFactory)
        {
            RedirectStandardOutput = true;
            FileName = "nuget";
        }

        public string Command { get; set; }        

        public override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            Arguments = Command;
            return base.ExecuteAsync(cancellationToken);
        }
    }
}