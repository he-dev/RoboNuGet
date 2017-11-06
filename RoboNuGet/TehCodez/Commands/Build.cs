using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Commander;
using Reusable.OmniLog;
using RoboNuGet.Data;

namespace RoboNuGet.Commands
{
    internal class Build : StartProcess
    {
        private readonly RoboNuGetFile _roboNuGetFile;
        private readonly IFileService _fileService;

        public Build(ILoggerFactory loggerFactory, RoboNuGetFile roboNuGetFile, IFileService fileService) : base(loggerFactory)
        {
            _roboNuGetFile = roboNuGetFile;
            _fileService = fileService;
            FileName = "msbuild";
        }
        
        public override Task  ExecuteAsync(CancellationToken cancellationToken)
        {
            var solutionFileName = _fileService.GetSolutionFileName(_roboNuGetFile.SolutionFileName);
            Arguments = _roboNuGetFile.MsBuild.ToString(solutionFileName);
            return base.ExecuteAsync(cancellationToken);
        }

    }    
}