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
using RoboNuGet.Data;
using RoboNuGet.Files;

namespace RoboNuGet.Commands
{
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

            foreach (var nuspecFile in nuspecFiles)
            {
                Logger.ConsoleParagraph(p => { });
                Logger.ConsoleParagraph(p => p.ConsoleText($"{Path.GetFileNameWithoutExtension(nuspecFile.FileName)} ({nuspecFile.Dependencies.Count()})"));

                foreach (var nuspecDependency in nuspecFile.Dependencies)
                {
                    Logger.ConsoleParagraph(p => p.ConsoleText($"- {nuspecDependency.Id} v{nuspecDependency.Version}"));
                }
            }

            return Task.CompletedTask;
        }
    }
}