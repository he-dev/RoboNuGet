using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using JetBrains.Annotations;
using Reusable.Commander;
using Reusable.Commander.Annotations;
using Reusable.Data.Annotations;
using Reusable.MarkupBuilder.Html;
using Reusable.OmniLog;
using Reusable.OmniLog.Abstractions;
using RoboNuGet.Files;
using RoboNuGet.Services;

namespace RoboNuGet.Commands
{
    internal class ListCommandLine : CommandLine
    {
        public ListCommandLine(CommandLineDictionary arguments) : base(arguments) { }

        [Description("Don't list dependencies.")]
        [Tags("s")]
        public bool Short => GetArgument(() => Short);

    }

    [Description("List packages.")]
    [Tags("lst", "l")]
    [UsedImplicitly]
    internal class List : Command<ListCommandLine>
    {
        private readonly RoboNuGetFile _roboNuGetFile;
        private readonly SolutionDirectoryTree _solutionDirectoryTree;

        public List
        (
            ILogger<List> logger, 
            RoboNuGetFile roboNuGetFile, 
            SolutionDirectoryTree solutionDirectoryTree
        ) : base(logger)
        {
            _roboNuGetFile = roboNuGetFile;
            _solutionDirectoryTree = solutionDirectoryTree;            
        }

        protected override Task ExecuteAsync(ListCommandLine commandLine, object context, CancellationToken cancellationToken)
        {
            var solution = _roboNuGetFile.SelectedSolutionSafe();
            var nuspecFiles = _solutionDirectoryTree.FindNuspecFiles(solution.DirectoryName);

            foreach (var nuspecFile in nuspecFiles.OrderBy(x => x.FileName))
            {
                var nuspecDirectoryName = Path.GetDirectoryName(nuspecFile.FileName);
                var packagesConfig = PackagesConfigFile.Load(nuspecDirectoryName);

                var csProj = CsProjFile.Load(Path.Combine(nuspecDirectoryName, $"{nuspecFile.Id}{CsProjFile.Extension}"));
                var projectDependencies = csProj.ProjectReferences.Select(projectReferenceName => new NuspecDependency { Id = projectReferenceName, Version = solution.FullVersion }).ToList();
                var packageDependencies = packagesConfig.Packages.Concat(csProj.PackageReferences).Select(package => new NuspecDependency { Id = package.Id, Version = package.Version }).ToList();

                var dependencyCount = projectDependencies.Count + packageDependencies.Count;

                //dependencyCount = nuspecFile.Dependencies.Count();

                if (!commandLine.Short)
                {
                    Logger.WriteLine(m => m);
                }
                Logger.WriteLine(m => m
                    .Indent()
                    .text($"{Path.GetFileNameWithoutExtension(nuspecFile.FileName)} ")
                    .span(s => s.text($"({dependencyCount})").color(ConsoleColor.Magenta)));

                if (!commandLine.Short)
                {
                    ListDependencies("Projects", projectDependencies.OrderBy(x => x.Id));
                    ListDependencies("Packages", packageDependencies.OrderBy(x => x.Id));
                }
            }

            return Task.CompletedTask;
        }

        private void ListDependencies(string header, IEnumerable<NuspecDependency> dependencies)
        {
            Logger.WriteLine(p => p.Indent(2).span(s => s.text($"[{header}]").color(ConsoleColor.DarkGray)));

            foreach (var nuspecDependency in dependencies)
            {
                Logger.WriteLine(p => p
                    .Indent(2)
                    .text($"- {nuspecDependency.Id} ")
                    .span(s => s.text($"v{nuspecDependency.Version}").color(ConsoleColor.DarkGray)));
            }
        }
    }
}