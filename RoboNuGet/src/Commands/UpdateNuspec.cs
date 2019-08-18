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
    [Tags("update", "u")]
    [UsedImplicitly]
    internal class UpdateNuspec : Command<UpdateNuspec.CommandLine>
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

        protected override Task ExecuteAsync(CommandLine commandLine, object context, CancellationToken cancellationToken)
        {
            var solution = _roboNuGetFile.SelectedSolutionSafe();
            //var nuspecFiles = _solutionDirectoryTree.FindNuspecFiles(solution.DirectoryName);

            var nuspecFileId = commandLine.NuspecFile;
            //var nuspecFile = nuspecFiles.Single(nf => nf.Id == nuspecFileId);
            var nuspecFile = _solutionDirectoryTree.GetNuspecFile(solution.DirectoryName, nuspecFileId);

            var nuspecDirectoryName = Path.GetDirectoryName(nuspecFile.FileName);
            var packagesConfig = PackagesConfigFile.Load(nuspecDirectoryName);
            var csProj = CsProjFile.Load(Path.Combine(nuspecDirectoryName, $"{nuspecFile.Id}{CsProjFile.Extension}"));

            var packageDependencies = packagesConfig.Packages.Concat(csProj.PackageReferences).Select(package => new NuspecDependency { Id = package.Id, Version = package.Version });
            var projectDependencies = csProj.ProjectReferences.Select(projectReferenceName => new NuspecDependency { Id = projectReferenceName, Version = commandLine.Version });

            nuspecFile.Dependencies = packageDependencies.Concat(projectDependencies);
            nuspecFile.Version = commandLine.Version;
            nuspecFile.Save();

            return Task.CompletedTask;
        }

        internal class CommandLine : CommandLineBase
        {
            public CommandLine(CommandLineDictionary arguments) : base(arguments) { }

            public string NuspecFile => GetArgument(() => NuspecFile);

            public string Version => GetArgument(() => Version);
        }
    }
}