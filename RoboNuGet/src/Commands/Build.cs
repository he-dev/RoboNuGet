using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable;
using Reusable.Commander;
using Reusable.IO;
using Reusable.OmniLog.Abstractions;
using RoboNuGet.Files;

namespace RoboNuGet.Commands
{
    [Description("Build the solution.")]
    [UsedImplicitly]
    internal class Build : Command<CommandLine>
    {
        private readonly RoboNuGetFile _roboNuGetFile;
        private readonly IDirectoryTree _directoryTree;

        public Build
        (
            ILogger<Build> logger,
            RoboNuGetFile roboNuGetFile,
            IDirectoryTree directoryTree
        ) : base(logger)
        {
            _roboNuGetFile = roboNuGetFile;
            _directoryTree = directoryTree;
        }

        protected override Task ExecuteAsync(CommandLine commandLine, object context, CancellationToken cancellationToken)
        {
            var arguments = _roboNuGetFile.MsBuild.ToString(_roboNuGetFile.SelectedSolutionSafe().FileName);
            var processExecutor = new ProcessExecutor();
            var result = processExecutor.ShellCmdExecute("/q /c pause |", "msbuild", arguments);
            return Task.CompletedTask;
        }
    }
}