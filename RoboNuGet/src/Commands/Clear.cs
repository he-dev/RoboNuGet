using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Custom;
using System.Text.RegularExpressions;
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

namespace RoboNuGet.Commands
{
    internal class ClearBag : SimpleBag
    {
        public string Option { get; set; }
    }

    [Description("Clear the console and refresh package list.")]
    [UsedImplicitly]
    [Alias("cls")]
    internal class Clear : ConsoleCommand<ClearBag>
    {
        private readonly RoboNuGetFile _roboNuGetFile;
        private readonly IDirectoryTree _directoryTree;

        public Clear
        (
            CommandServiceProvider<Clear> serviceProvider,
            RoboNuGetFile roboNuGetFile,
            IDirectoryTree directoryTree
        ) : base(serviceProvider)
        {
            _roboNuGetFile = roboNuGetFile;
            _directoryTree = directoryTree;
        }

        protected override Task ExecuteAsync(ClearBag parameter, CancellationToken cancellationToken)
        {
            Console.Clear();
            RenderSplashScreen(_roboNuGetFile, parameter.Option);
            return Task.CompletedTask;
        }

        private void RenderSplashScreen(RoboNuGetFile roboNuGetFile, string option)
        {
            Logger.WriteLine(m => m.Prompt().span(s => s.text("RoboNuGet v6.0.0").color(ConsoleColor.DarkGray)));

            var showAll = SoftString.Comparer.Equals(option, "selection");
            var solutions = showAll ? roboNuGetFile.Solutions : new[] { _roboNuGetFile.SelectedSolution };

            foreach (var (solution, index) in solutions.Select((s, i) => (s, i)))
            {
                var nuspecFiles = _directoryTree.FindNuspecFiles(roboNuGetFile).ToList();

                Logger.WriteLine(
                    p => p
                        .Prompt()
                        .text($"Solution {(showAll ? $"[{index}]" : string.Empty)}")
                        .span(s => s.text(Path.GetFileNameWithoutExtension(solution.FileName).QuoteWith("'")).color(ConsoleColor.Yellow))
                        .text(" ")
                        .span(s => s.text($"v{solution.FullVersion}").color(ConsoleColor.Magenta))
                        .text(" ")
                        .text($"({nuspecFiles.Count} package{(nuspecFiles.Count != 1 ? "s" : string.Empty)})")
                );
            }
        }
    }

    internal static class DirectoryTreeExtensions
    {
        [NotNull, ItemNotNull]
        public static IEnumerable<NuspecFile> FindNuspecFiles(this IDirectoryTree directoryTree, RoboNuGetFile roboNuGetFile)
        {
            if (roboNuGetFile.SelectedSolution is null)
            {
                throw new ArgumentException("Solution not selected.");
            }

            var pattern = roboNuGetFile.ExcludeDirectories.Select(Regex.Escape).Join("|");

            return
                directoryTree
                    .WalkSilently(roboNuGetFile.SelectedSolution.DirectoryName)
                    .SkipDirectories($"\\({pattern})")
                    .WhereFiles("\\.nuspec$")
                    .SelectMany(node => node.FileNames.Select(name => Path.Combine(node.DirectoryName, name)))
                    .Select(NuspecFile.Load);
        }
    }

    internal class SelectBag : SimpleBag
    {
        [Position(1)]
        public int Solution { get; set; }
    }

    internal class Select : ConsoleCommand<SelectBag>
    {
        private readonly RoboNuGetFile _roboNuGetFile;

        public Select(ICommandServiceProvider serviceProvider, RoboNuGetFile roboNuGetFile) : base(serviceProvider)
        {
            _roboNuGetFile = roboNuGetFile;
        }

        protected override Task ExecuteAsync(SelectBag parameter, CancellationToken cancellationToken)
        {
            var solution = _roboNuGetFile.Solutions.ElementAtOrDefault(parameter.Solution);
            if (!(solution is null))
            {
                _roboNuGetFile.SelectedSolution = solution;
            }
            return Task.CompletedTask;
        }
    }
}