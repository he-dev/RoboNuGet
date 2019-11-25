using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Commander;
using Reusable.Data.Annotations;
using Reusable.OmniLog.Abstractions;
using RoboNuGet.Files;
using RoboNuGet.Services;
using t = RoboNuGet.ConsoleTemplates;

namespace RoboNuGet.Commands
{
    

    [Description("List packages.")]
    [Alias("lst", "l")]
    [UsedImplicitly]
    internal class List : Command<List.Parameter>
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

        protected override Task ExecuteAsync(Parameter parameter, CancellationToken cancellationToken)
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

                if (!parameter.Short)
                {
                    Logger.WriteLine();
                }

                Logger.WriteLine(new t.PackageInfo
                {
                    PackageId = Path.GetFileNameWithoutExtension(nuspecFile.FileName),
                    DependencyCount = dependencyCount
                });

                if (!parameter.Short)
                {
                    ListDependencies("Projects", projectDependencies.OrderBy(x => x.Id));
                    ListDependencies("Packages", packageDependencies.OrderBy(x => x.Id));
                }
            }

            return Task.CompletedTask;
        }

        private void ListDependencies(string header, IEnumerable<NuspecDependency> dependencies)
        {
            Logger.WriteLine(new t.PackageDependencySection { Name = header });

            foreach (var nuspecDependency in dependencies)
            {
                Logger.WriteLine(new t.PackageDependencyInfo { Name = nuspecDependency.Id, Version = nuspecDependency.Version });
            }
        }
        
        internal class Parameter : CommandParameter
        {
            [Description("Don't list dependencies.")]
            [Alias("s")]
            public bool Short { get; set; }
        }
    }
}