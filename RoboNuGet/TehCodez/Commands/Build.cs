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

        public Build(ILoggerFactory loggerFactory, RoboNuGetFile roboNuGetFile) : base(loggerFactory)
        {
            _roboNuGetFile = roboNuGetFile;
            FileName = "msbuild";
        }
        
        public override Task  ExecuteAsync(CancellationToken cancellationToken)
        {
            Arguments = _roboNuGetFile.MsBuild.ToString(_roboNuGetFile.SolutionFileName);
            return base.ExecuteAsync(cancellationToken);
        }

    }    
}