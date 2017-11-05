using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Reusable.Commander;
using Reusable.OmniLog;
using RoboNuGet.Data;

namespace RoboNuGet.Commands
{
    internal class Patch : ConsoleCommand
    {
        private readonly RoboNuGetFile _roboNuGetFile;

        public Patch(ILoggerFactory loggerFactory, RoboNuGetFile roboNuGetFile) : base(loggerFactory)
        {
            _roboNuGetFile = roboNuGetFile;
        }

        public override Task ExecuteAsync(CancellationToken cancellationToken)
        {
//            var config = (RoboNuGetFile)parameter.Config;
//            config.IncrementPatchVersion();
//            config.Save();
            
            
            return Task.CompletedTask;
        }
    }
}