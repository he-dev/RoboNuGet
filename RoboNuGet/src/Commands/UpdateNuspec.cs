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
using Reusable.OmniLog;
using RoboNuGet.Files;

namespace RoboNuGet.Commands
{
    internal class UpdateNuspecBag : SimpleBag
    {
        [NotMapped]
        public NuspecFile NuspecFile { get; set; }

        public string Version { get; set; }
    }

    [Alias("update", "u")]
    [UsedImplicitly]
    internal class UpdateNuspec : ConsoleCommand<UpdateNuspecBag>
    {
        public UpdateNuspec(CommandServiceProvider<UpdateNuspec> serviceProvider)
            : base(serviceProvider)
        { }


        protected override Task ExecuteAsync(UpdateNuspecBag parameter, CancellationToken cancellationToken)
        {
            var nuspecDirectoryName = Path.GetDirectoryName(parameter.NuspecFile.FileName);
            var packagesConfig = PackagesConfigFile.Load(nuspecDirectoryName);
            var csProj = CsProjFile.Load(Path.Combine(nuspecDirectoryName, $"{parameter.NuspecFile.Id}{CsProjFile.Extension}"));

            var packageDependencies = packagesConfig.Packages.Concat(csProj.PackageReferences).Select(package => new NuspecDependency { Id = package.Id, Version = package.Version });
            var projectDependencies = csProj.ProjectReferences.Select(projectReferenceName => new NuspecDependency { Id = projectReferenceName, Version = parameter.Version });

            parameter.NuspecFile.Dependencies = packageDependencies.Concat(projectDependencies);
            parameter.NuspecFile.Version = parameter.Version;
            parameter.NuspecFile.Save();

            return Task.CompletedTask;
        }
    }
}