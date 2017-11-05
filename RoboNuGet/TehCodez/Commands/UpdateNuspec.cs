using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Reusable.Commander;
using Reusable.OmniLog;
using RoboNuGet.Data;
using RoboNuGet.Files;

namespace RoboNuGet.Commands
{
    internal class UpdateNuspec : ConsoleCommand
    {
        public UpdateNuspec(ILoggerFactory loggerFactory) : base(loggerFactory)
        {
        }

        public NuspecFile NuspecFile { get; set; }
        
        public string Version { get; set; }

        public override Task  ExecuteAsync(CancellationToken cancellationToken)
        {
            var directory = Path.GetDirectoryName(NuspecFile.FileName);
            var packagesConfig = PackagesConfigFile.From(directory);
            var csProj = CsProjFile.From(directory);

            NuspecFile.ClearDependencies();
            foreach (var package in packagesConfig.Packages)
            {
                NuspecFile.AddDependency(package.Id, package.Version);
            }

            foreach (var projectReferenceName in csProj.ProjectReferenceNames)
            {
                NuspecFile.AddDependency(projectReferenceName, Version);
            }

            NuspecFile.Version = Version;
            NuspecFile.Save();
            
            return Task.CompletedTask;
        }        
    }
}