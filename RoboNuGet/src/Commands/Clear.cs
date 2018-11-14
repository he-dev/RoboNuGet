using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable;
using Reusable.Commander;
using Reusable.Commander.Annotations;
using Reusable.Extensions;
using Reusable.MarkupBuilder.Html;
using Reusable.OmniLog;
using RoboNuGet.Files;
using RoboNuGet.Services;

namespace RoboNuGet.Commands
{
    internal class ClearBag : SimpleBag
    {
        [Description("Clear solution selection.")]
        [Alias("s")]
        public bool Selection { get; set; }
    }

    [Description("Clear the console and refresh package list.")]
    [UsedImplicitly]
    [Alias("cls")]
    internal class Clear : ConsoleCommand<ClearBag>
    {
        private readonly RoboNuGetFile _roboNuGetFile;
        private readonly SolutionDirectoryTree _solutionDirectoryTree;

        public Clear
        (
            CommandServiceProvider<Clear> serviceProvider,
            RoboNuGetFile roboNuGetFile,
            SolutionDirectoryTree solutionDirectoryTree
        ) : base(serviceProvider)
        {
            _roboNuGetFile = roboNuGetFile;
            _solutionDirectoryTree = solutionDirectoryTree;
        }

        protected override Task ExecuteAsync(ClearBag parameter, CancellationToken cancellationToken)
        {
            Console.Clear();
            if (parameter.Selection)
            {
                _roboNuGetFile.SelectedSolution = default;
            }
            RenderSplashScreen();
            return Task.CompletedTask;
        }

        private void RenderSplashScreen()
        {
            Logger.WriteLine(m => m.Prompt().span(s => s.text("RoboNuGet v6.0.0").color(ConsoleColor.DarkGray)));

            var solutionSelected = !(_roboNuGetFile.SelectedSolution is null);
            var solutions = !solutionSelected ? _roboNuGetFile.Solutions : new[] { _roboNuGetFile.SelectedSolution };

            foreach (var (solution, index) in solutions.Select((s, i) => (s, i)))//.OrderBy(t => Path.GetFileNameWithoutExtension(t.s.FileName), StringComparer.OrdinalIgnoreCase))
            {
                var nuspecFiles = _solutionDirectoryTree.FindNuspecFiles(solution.DirectoryName).ToList();

                Logger.WriteLine(
                    p => p
                        .Prompt()
                        .text($"Solution {(!solutionSelected ? $"[{index}] " : string.Empty)}")
                        .span(s => s.text(Path.GetFileNameWithoutExtension(solution.FileName).QuoteWith("'")).color(ConsoleColor.Yellow))
                        .text(" ")
                        .span(s => s.text($"v{solution.FullVersion}").color(ConsoleColor.Magenta))
                        .text(" ")
                        .text($"({nuspecFiles.Count} package{(nuspecFiles.Count != 1 ? "s" : string.Empty)})")
                );
            }

            if (!solutionSelected)
            {
                Logger.WriteLine(p => p.Prompt().text("Use the 'select' command to pick a solution."));
            }
        }
    }
}