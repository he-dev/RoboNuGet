using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable;
using Reusable.Commander;
using Reusable.OmniLog.Abstractions;
using RoboNuGet.Files;

namespace RoboNuGet.Commands
{
    [Description("Build the solution.")]
    [UsedImplicitly]
    internal class Build : Command<CommandParameter>
    {
        private readonly Session _session;

        public Build
        (
            ILogger<Build> logger,
            Session session
        ) : base(logger)
        {
            _session = session;
        }

        protected override Task ExecuteAsync(CommandParameter parameter, CancellationToken cancellationToken)
        {
            var arguments = _session.SolutionOrThrow().MsBuild.RenderArgs(_session.SolutionOrThrow().FileName);
            var processExecutor = new ProcessExecutor();
            var result = processExecutor.ShellCmdExecute("/q /c pause |", "msbuild", arguments);
            return Task.CompletedTask;
        }
    }
}