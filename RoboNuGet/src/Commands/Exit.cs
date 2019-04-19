using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using JetBrains.Annotations;
using Reusable.Commander;
using Reusable.Commander.Services;
using Reusable.OmniLog;

namespace RoboNuGet.Commands
{
    [Description("Exit RoboNuGet.")]
    [UsedImplicitly]
    internal class Exit : ConsoleCommand
    {
        public Exit(CommandServiceProvider<Exit> serviceProvider) : base(serviceProvider, nameof(Exit)) { }

        protected override Task ExecuteAsync(ICommandLineReader<ICommandParameter> parameter, NullContext context, CancellationToken cancellationToken)
        {
            Environment.Exit(0);
            return Task.CompletedTask;
        }
    }
}