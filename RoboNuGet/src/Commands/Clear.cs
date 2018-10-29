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
using Reusable.Commander;
using Reusable.Commander.Annotations;
using Reusable.Extensions;
using Reusable.MarkupBuilder.Html;
using Reusable.OmniLog;
using RoboNuGet.Files;

namespace RoboNuGet.Commands
{
    [Description("Clear the console and refresh package list.")]
    [UsedImplicitly]
    [Alias("cls")]
    internal class Clear : ConsoleCommand<SimpleBag>
    {
        private readonly RoboNuGetFile _roboNuGetFile;
        private readonly IDirectoryTree _directoryTree;

        public Clear(
            CommandServiceProvider<Clear> serviceProvider,
            RoboNuGetFile roboNuGetFile,
            IDirectoryTree directoryTree
        ) : base(serviceProvider)
        {
            _roboNuGetFile = roboNuGetFile;
            _directoryTree = directoryTree;
        }

        protected override Task ExecuteAsync(SimpleBag parameter, CancellationToken cancellationToken)
        {
            Console.Clear();
            RenderSplashScreen(_roboNuGetFile);
            return Task.CompletedTask;
        }

        private void RenderSplashScreen(RoboNuGetFile roboNuGetFile)
        {
            Logger.WriteLine(m => m.Prompt().span(s => s.text("RoboNuGet v6.0.0").color(ConsoleColor.DarkGray)));

            foreach (var solution in roboNuGetFile.Solutions)
            {
                //var solutionFileName = _fileSearch.FindSolutionFile();
                var nuspecFiles = _directoryTree.FindNuspecFiles(solution.DirectoryName, roboNuGetFile.ExcludeDirectories).ToList();

                Logger.WriteLine(p => p
                    .Prompt()
                    .text("Solution ")
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
        public static IEnumerable<string> FindNuspecFiles(this IDirectoryTree directoryTree, string path, IEnumerable<string> excludeDirectories)
        {
            var pattern = excludeDirectories.Select(Regex.Escape).Join("|");

            return
                directoryTree
                    .WalkSilently(path)
                    .SkipDirectories($"\\({pattern})")
                    .WhereFiles("\\.nuspec$")
                    .SelectMany(node => node.FileNames.Select(name => Path.Combine(node.DirectoryName, name)));
        }
    }

    internal class SelectBag : SimpleBag
    {
        [Alias("pkg")]
        public string Package { get; set; }
    }

    internal class Select : ConsoleCommand<SelectBag>
    {
        private readonly Selection _selection;

        public Select(ICommandServiceProvider serviceProvider, Selection selection) : base(serviceProvider)
        {
            _selection = selection;
        }

        protected override Task ExecuteAsync(SelectBag parameter, CancellationToken cancellationToken)
        {
            _selection.Package = parameter.Package;
            return Task.CompletedTask;
        }
    }

    internal class Selection
    {
        public string Package { get; set; }
    }
}