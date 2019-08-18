using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Commander;
using Reusable.Data.Annotations;
using Reusable.OmniLog.Abstractions;
using t = RoboNuGet.ConsoleTemplates;
using RoboNuGet.Files;
using RoboNuGet.Services;

namespace RoboNuGet.Commands
{
    [Description("Clear the console and refresh package list.")]
    [UsedImplicitly]
    [Tags("cls")]
    internal class Clear : Command<Clear.CommandLine>
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

        protected override Task ExecuteAsync(CommandLine commandLine, object context, CancellationToken cancellationToken)
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
            Logger.WriteLine(new t.Prompt(), new t.ProgramInfo());

            if (_roboNuGetFile.SelectedSolution is null)
            {
                foreach (var (solution, index) in _roboNuGetFile.Solutions.Select((s, i) => (s, i + 1)))
                {
                    var nuspecFiles = _solutionDirectoryTree.FindNuspecFiles(solution.DirectoryName).ToList();

                    Logger.WriteLine(new t.Indent(1), new t.Clear.SolutionOption
                    {
                        Index = index,
                        Name = Path.GetFileNameWithoutExtension(solution.FileName),
                        Version = solution.FullVersion,
                        NuspecFileCount = nuspecFiles.Count
                    });
                }

                Logger.WriteLine(new t.Indent(1), new t.Clear.AskForSolution());
            }
            else
            {
                var nuspecFiles = _solutionDirectoryTree.FindNuspecFiles(_roboNuGetFile.SelectedSolution.DirectoryName).ToList();

                Logger.WriteLine(new t.Indent(1), new t.Clear.SolutionSelection
                {
                    Name = Path.GetFileNameWithoutExtension(_roboNuGetFile.SelectedSolution.FileName),
                    Version = _roboNuGetFile.SelectedSolution.FullVersion,
                    NuspecFileCount = nuspecFiles.Count
                });
            }
        }

        internal class CommandLine : CommandLineBase
        {
            public CommandLine(CommandLineDictionary arguments) : base(arguments) { }

            [Description("Clear solution selection.")]
            [Tags("s")]
            public bool Selection => GetArgument(() => Selection);
        }
    }
}