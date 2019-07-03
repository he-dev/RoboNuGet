using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using JetBrains.Annotations;
using Reusable.Commander;
using Reusable.OmniLog;
using Reusable.OmniLog.Abstractions;

namespace RoboNuGet.Commands
{
    [Description("Exit RoboNuGet.")]
    [UsedImplicitly]
    internal class Exit : Command<CommandLine>
    {
        public Exit(ILogger<Exit> logger) : base(logger) { }

        protected override Task ExecuteAsync(CommandLine commandLine, object context, CancellationToken cancellationToken)
        {
            Environment.Exit(0);
            return Task.CompletedTask;
        }
    }
}