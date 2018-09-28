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
    internal class Exit : ConsoleCommand<SimpleBag>
    {
        public Exit(ILogger<Exit> logger, ICommandLineMapper mapper) : base(logger, mapper)
        {
        }

        protected override Task ExecuteAsync(SimpleBag parameter, CancellationToken cancellationToken)
        {
            Environment.Exit(0);
            return Task.CompletedTask;
        }
    }
}