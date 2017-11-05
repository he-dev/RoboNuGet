using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using JetBrains.Annotations;
using Reusable.Commander;
using Reusable.OmniLog;

namespace RoboNuGet.Commands
{
    [UsedImplicitly]
    internal class Exit : ConsoleCommand
    {
        public Exit(ILoggerFactory loggerFactory) : base(loggerFactory)
        {
        }

        public override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            Environment.Exit(0);
            return Task.CompletedTask;
        }
    }
}