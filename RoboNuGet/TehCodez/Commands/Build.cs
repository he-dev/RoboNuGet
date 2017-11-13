using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Commander;
using Reusable.OmniLog;
using RoboNuGet.Files;

namespace RoboNuGet.Commands
{
    [UsedImplicitly]
    internal class Build : StartProcess
    {
        private readonly RoboNuGetFile _roboNuGetFile;
        private readonly IFileSearch _fileSearch;

        public Build(ILoggerFactory loggerFactory, RoboNuGetFile roboNuGetFile, IFileSearch fileSearch) : base(loggerFactory)
        {
            _roboNuGetFile = roboNuGetFile;
            _fileSearch = fileSearch;
            FileName = "msbuild";
        }

        public override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var solutionFileName = _fileSearch.FindSolutionFile();
            Arguments = _roboNuGetFile.MsBuild.ToString(solutionFileName);
            return base.ExecuteAsync(cancellationToken);
        }
    }
}