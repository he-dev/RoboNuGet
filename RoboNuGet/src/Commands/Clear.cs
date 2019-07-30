using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable;
using Reusable.Commander;
using Reusable.Commander.Annotations;
using Reusable.Data.Annotations;
using Reusable.Extensions;
using Reusable.MarkupBuilder.Html;
using Reusable.OmniLog;
using Reusable.OmniLog.Abstractions;
using RoboNuGet.Files;
using RoboNuGet.Services;

namespace RoboNuGet.Commands
{
    internal class ClearCommandLine : CommandLine
    {
        public ClearCommandLine(CommandLineDictionary arguments) : base(arguments) { }

        [Description("Clear solution selection.")]
        [Tags("s")]
        public bool Selection => GetArgument(() => Selection);
    }

    [Description("Clear the console and refresh package list.")]
    [UsedImplicitly]
    [Tags("cls")]
    internal class Clear : Command<ClearCommandLine>
    {
        private readonly RoboNuGetFile _roboNuGetFile;
        private readonly SolutionDirectoryTree _solutionDirectoryTree;

        public Clear
        (
            ILogger<Clear> logger,
            RoboNuGetFile roboNuGetFile,
            SolutionDirectoryTree solutionDirectoryTree
        ) : base(logger)
        {
            _roboNuGetFile = roboNuGetFile;
            _solutionDirectoryTree = solutionDirectoryTree;
        }

        protected override Task ExecuteAsync(ClearCommandLine commandLine, object context, CancellationToken cancellationToken)
        {
            System.Console.Clear();
            if (commandLine.Selection)
            {
                _roboNuGetFile.SelectedSolution = default;
            }

            RenderSplashScreen();
            return Task.CompletedTask;
        }

        private void RenderSplashScreen()
        {
            Logger.Console().Log(new RoboNuGet.Console.Models.ProgramInfo());

            var solutionSelected = !(_roboNuGetFile.SelectedSolution is null);
            var solutions = !solutionSelected ? _roboNuGetFile.Solutions : new[] { _roboNuGetFile.SelectedSolution };

            foreach (var (solution, index) in solutions.Select((s, i) => (s, i + 1))) //.OrderBy(t => Path.GetFileNameWithoutExtension(t.s.FileName), StringComparer.OrdinalIgnoreCase))
            {
                var nuspecFiles = _solutionDirectoryTree.FindNuspecFiles(solution.DirectoryName).ToList();

                Logger.Console().Log(new Console.Models.SolutionInfo
                {
                    Name = Path.GetFileNameWithoutExtension(solution.FileName),
                    Version = solution.FullVersion,
                    NuspecFileCount = nuspecFiles.Count
                });
            }

            if (!solutionSelected)
            {
                Logger.Console().Log(new Console.Models.SelectSolution());
            }
        }
    }
}