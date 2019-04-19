using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using JetBrains.Annotations;
using Reusable.Commander;
using Reusable.Commander.Annotations;
using Reusable.Commander.Services;
using Reusable.OmniLog;
using RoboNuGet.Files;
using RoboNuGet.Services;

namespace RoboNuGet.Commands
{
    internal interface IUpdateNuspecParameter : ICommandParameter
    {
        string NuspecFile { get; }

        string Version { get; }
    }

    [Internal]
    [Alias("update", "u")]
    [UsedImplicitly]
    internal class UpdateNuspec : ConsoleCommand<IUpdateNuspecParameter>
    {
        private readonly RoboNuGetFile _roboNuGetFile;
        private readonly SolutionDirectoryTree _solutionDirectoryTree;

        public UpdateNuspec
        (
            CommandServiceProvider<UpdateNuspec> serviceProvider,
            RoboNuGetFile roboNuGetFile,
            SolutionDirectoryTree solutionDirectoryTree
        )
            : base(serviceProvider)
        {
            _roboNuGetFile = roboNuGetFile;
            _solutionDirectoryTree = solutionDirectoryTree;
        }

        protected override Task ExecuteAsync(ICommandLineReader<IUpdateNuspecParameter> parameter, NullContext context, CancellationToken cancellationToken)
        {
            var solution = _roboNuGetFile.SelectedSolutionSafe();
            var nuspecFiles = _solutionDirectoryTree.FindNuspecFiles(solution.DirectoryName);
            var nuspecFileId = parameter.GetItem(x => x.NuspecFile);
            var nuspecFile = nuspecFiles.Single(nf => nf.Id == nuspecFileId);
            
            var nuspecDirectoryName = Path.GetDirectoryName(nuspecFile.FileName);
            var packagesConfig = PackagesConfigFile.Load(nuspecDirectoryName);
            var csProj = CsProjFile.Load(Path.Combine(nuspecDirectoryName, $"{nuspecFile.Id}{CsProjFile.Extension}"));

            var packageDependencies = packagesConfig.Packages.Concat(csProj.PackageReferences).Select(package => new NuspecDependency { Id = package.Id, Version = package.Version });
            var projectDependencies = csProj.ProjectReferences.Select(projectReferenceName => new NuspecDependency { Id = projectReferenceName, Version = parameter.GetItem(x => x.Version) });

            nuspecFile.Dependencies = packageDependencies.Concat(projectDependencies);
            nuspecFile.Version = parameter.GetItem(x => x.Version);
            nuspecFile.Save();

            return Task.CompletedTask;
        }
    }
}