using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using JetBrains.Annotations;
using Reusable.Commander;
using Reusable.OmniLog;
using RoboNuGet.Files;

namespace RoboNuGet.Commands
{
    [Alias("update", "u")]
    [UsedImplicitly]
    internal class UpdateNuspec : ConsoleCommand
    {
        public UpdateNuspec(ILoggerFactory loggerFactory) : base(loggerFactory)
        {
        }

        public NuspecFile NuspecFile { get; set; }

        public string Version { get; set; }

        public override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var nuspecDirectoryName = Path.GetDirectoryName(NuspecFile.FileName);
            var packagesConfig = PackagesConfigFile.Load(nuspecDirectoryName);
            var csProj = CsProjFile.Load(Path.Combine(nuspecDirectoryName, $"{NuspecFile.Id}{CsProjFile.DefaultExtension}"));

            var packageDependencies = packagesConfig.Packages.Concat(csProj.PackageReferences).Select(package => new NuspecDependency { Id = package.Id, Version = package.Version });
            var projectDependencies = csProj.ProjectReferences.Select(projectReferenceName => new NuspecDependency { Id = projectReferenceName, Version = Version });

            NuspecFile.Dependencies = packageDependencies.Concat(projectDependencies);
            NuspecFile.Version = Version;
            NuspecFile.Save();

            return Task.CompletedTask;
        }
    }
}