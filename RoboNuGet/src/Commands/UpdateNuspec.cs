using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Commander;
using Reusable.Commander.Annotations;
using Reusable.Data.Annotations;
using Reusable.OmniLog.Abstractions;
using RoboNuGet.Files;
using RoboNuGet.Services;

namespace RoboNuGet.Commands
{
    [Internal]
    [Alias("update", "u")]
    [UsedImplicitly]
    internal class UpdateNuspec : Command<UpdateNuspec.Parameter>
    {
        private readonly RoboNuGetFile _roboNuGetFile;
        private readonly SolutionDirectoryTree _solutionDirectoryTree;

        public UpdateNuspec
        (
            ILogger<UpdateNuspec> logger,
            RoboNuGetFile roboNuGetFile,
            SolutionDirectoryTree solutionDirectoryTree
        )
            : base(logger)
        {
            _roboNuGetFile = roboNuGetFile;
            _solutionDirectoryTree = solutionDirectoryTree;
        }

        protected override Task ExecuteAsync(Parameter parameter, CancellationToken cancellationToken)
        {
            var solution = _roboNuGetFile.SelectedSolutionSafe();
            //var nuspecFiles = _solutionDirectoryTree.FindNuspecFiles(solution.DirectoryName);

            var nuspecFileId = parameter.NuspecFile;
            //var nuspecFile = nuspecFiles.Single(nf => nf.Id == nuspecFileId);
            var nuspecFile = _solutionDirectoryTree.GetNuspecFile(solution.DirectoryName, nuspecFileId);

            var nuspecDirectoryName = Path.GetDirectoryName(nuspecFile.FileName);
            var packagesConfig = PackagesConfigFile.Load(nuspecDirectoryName);
            var csProj = CsProjFile.Load(Path.Combine(nuspecDirectoryName, $"{nuspecFile.Id}{CsProjFile.Extension}"));

            var packageDependencies = packagesConfig.Packages.Concat(csProj.PackageReferences).Select(package => new NuspecDependency { Id = package.Id, Version = package.Version });
            var projectDependencies = csProj.ProjectReferences.Select(projectReferenceName => new NuspecDependency { Id = projectReferenceName, Version = parameter.Version });

            nuspecFile.Dependencies = packageDependencies.Concat(projectDependencies);
            nuspecFile.Version = parameter.Version;
            nuspecFile.Save();

            return Task.CompletedTask;
        }

        internal class Parameter : CommandParameter
        {
            public string NuspecFile { get; set; }

            public string Version { get; set; }
        }
    }
}