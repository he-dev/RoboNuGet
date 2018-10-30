using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable;
using Reusable.Commander;
using Reusable.Extensions;
using Reusable.OmniLog;
using RoboNuGet.Files;

namespace RoboNuGet.Commands
{
    [Description("Build the solution.")]
    [UsedImplicitly]
    internal class Build : ConsoleCommand<SimpleBag>
    {
        private readonly RoboNuGetFile _roboNuGetFile;
        private readonly IDirectoryTree _directoryTree;

        public Build
        (
            CommandServiceProvider<Build> serviceProvider,
            RoboNuGetFile roboNuGetFile,
            IDirectoryTree directoryTree
        ) : base(serviceProvider)
        {
            _roboNuGetFile = roboNuGetFile;
            _directoryTree = directoryTree;
        }

        protected override Task ExecuteAsync(SimpleBag parameter, CancellationToken cancellationToken)
        {
            var arguments = _roboNuGetFile.MsBuild.ToString(_roboNuGetFile.SelectedSolutionSafe().FileName);
            var processExecutor = new ProcessExecutor();
            var result = processExecutor.ShellCmdExecute("/q /c pause |", "msbuild", arguments);
            return Task.CompletedTask;
        }
    }
}