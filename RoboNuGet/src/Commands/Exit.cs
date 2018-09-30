using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using JetBrains.Annotations;
using Reusable.Commander;
using Reusable.OmniLog;

namespace RoboNuGet.Commands
{
    [Description("Exit RoboNuGet.")]
    [UsedImplicitly]
    internal class Exit : ConsoleCommand<SimpleBag>
    {
        public Exit(CommandServiceProvider<Exit> serviceProvider) : base(serviceProvider)
        {
        }

        protected override Task ExecuteAsync(SimpleBag parameter, CancellationToken cancellationToken)
        {
            Environment.Exit(0);
            return Task.CompletedTask;
        }
    }
}