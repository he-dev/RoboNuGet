using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Commander;
using Reusable.OmniLog.Abstractions;

namespace RoboNuGet.Commands
{
    [Description("Exit RoboNuGet.")]
    [UsedImplicitly]
    internal class Exit : Command<CommandParameter>
    {
        protected override Task ExecuteAsync(CommandParameter parameter, CancellationToken cancellationToken)
        {
            Environment.Exit(0);
            return Task.CompletedTask;
        }
    }
}