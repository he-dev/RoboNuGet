using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable;
using Reusable.Commander;
using Reusable.Extensions;
using Reusable.OmniLog;
using RoboNuGet.Files;

namespace RoboNuGet.Commands
{
    [UsedImplicitly]
    internal class Build : ConsoleCommand
    {
        private readonly RoboNuGetFile _roboNuGetFile;
        private readonly IFileSearch _fileSearch;

        public Build(ILoggerFactory loggerFactory, RoboNuGetFile roboNuGetFile, IFileSearch fileSearch) : base(loggerFactory)
        {
            _roboNuGetFile = roboNuGetFile;
            _fileSearch = fileSearch;
        }

        public override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var solutionFileName = _fileSearch.FindSolutionFile();
            var arguments = _roboNuGetFile.MsBuild.ToString(solutionFileName);
            
            var processExecutor = new ProcessExecutor();

            var result = processExecutor.ShellCmdExecute("/q /c pause |", "msbuild", arguments);
            
            return Task.CompletedTask;
        }        
    }
}