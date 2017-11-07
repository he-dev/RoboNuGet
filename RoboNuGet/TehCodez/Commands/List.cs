using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using JetBrains.Annotations;
using Reusable.Commander;
using Reusable.ConsoleColorizer;
using Reusable.OmniLog;
using RoboNuGet.Files;

namespace RoboNuGet.Commands
{
    [UsedImplicitly]
    internal class List : ConsoleCommand
    {
        private readonly RoboNuGetFile _roboNuGetFile;
        private readonly IFileService _fileService;

        public List(ILoggerFactory loggerFactory, RoboNuGetFile roboNuGetFile, IFileService fileService) : base(loggerFactory)
        {
            _roboNuGetFile = roboNuGetFile;
            _fileService = fileService;
        }

        public override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var solutionFileName = _fileService.GetSolutionFileName(_roboNuGetFile.SolutionFileName);
            var nuspecFiles = _fileService.GetNuspecFiles(Path.GetDirectoryName(solutionFileName));

            foreach (var nuspecFile in nuspecFiles.OrderBy(x => x.FileName))
            {
                Logger.ConsoleParagraph(p => { });
                Logger.ConsoleParagraph(p => p
                    .Indent()
                    .ConsoleText($"{Path.GetFileNameWithoutExtension(nuspecFile.FileName)} ")
                    .ConsoleSpan(ConsoleColor.Magenta, null, s => s.ConsoleText($"({nuspecFile.Dependencies.Count()})"))
                );

                var nuspecDirectoryName = Path.GetDirectoryName(nuspecFile.FileName);
                var packagesConfig = PackagesConfigFile.Load(nuspecDirectoryName);
                var csProj = CsProjFile.Load(Path.Combine(nuspecDirectoryName, $"{nuspecFile.Id}{CsProjFile.DefaultExtension}"));

                Logger.ConsoleParagraph(p => p.Indent(2).ConsoleSpan(ConsoleColor.DarkGray, null, s => s.ConsoleText($"[Projects]")));

                var projectDependencies = csProj.ProjectReferences.Select(projectReferenceName => new NuspecDependency(projectReferenceName, _roboNuGetFile.FullVersion));
                foreach (var nuspecDependency in projectDependencies.OrderBy(x => x.Id))
                {
                    Logger.ConsoleParagraph(p => p
                        .Indent(2)
                        .ConsoleText($"- {nuspecDependency.Id} ")
                        .ConsoleSpan(ConsoleColor.DarkGray, null, s => s.ConsoleText($"v{nuspecDependency.Version}"))
                    );
                }

                Logger.ConsoleParagraph(p => p.Indent(2).ConsoleSpan(ConsoleColor.DarkGray, null, s => s.ConsoleText($"[Packages]")));

                var packageDependencies = packagesConfig.Packages.Select(package => new NuspecDependency(package.Id, package.Version));
                foreach (var nuspecDependency in packageDependencies.OrderBy(x => x.Id))
                {
                    Logger.ConsoleParagraph(p => p
                        .Indent(2)
                        .ConsoleText($"- {nuspecDependency.Id} ")
                        .ConsoleSpan(ConsoleColor.DarkGray, null, s => s.ConsoleText($"v{nuspecDependency.Version}"))
                    );
                }
            }


            return Task.CompletedTask;
        }
    }
}