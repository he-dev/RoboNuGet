using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Reusable.Commander;
using Reusable.CommandLine;
using Reusable.OmniLog;
using RoboNuGet.Data;

namespace RoboNuGet.Commands
{
    [Alias("ver", "v")]
    internal class Version : ConsoleCommand
    {
        private readonly RoboNuGetFile _roboNuGetFile;

        public Version(ILoggerFactory loggerFactory, RoboNuGetFile roboNuGetFile) : base(loggerFactory)
        {
            _roboNuGetFile = roboNuGetFile;
        }

        [Parameter(Position = 1)]
        public string NewVersion { get; set; }

        public override Task  ExecuteAsync(CancellationToken cancellationToken)
        {
            // todo: needs validation with semantic version

            _roboNuGetFile.PackageVersion = NewVersion;
            _roboNuGetFile.Save();

            return Task.CompletedTask;
        }
    }
}