using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Reusable.OmniLog;
using RoboNuGet.Files;

namespace RoboNuGet.Commands
{
    internal abstract class NuGet : StartProcess
    {
        protected NuGet(ILoggerFactory loggerFactory, RoboNuGetFile roboNuGetFile) : base(loggerFactory)
        {
            RoboNuGetFile = roboNuGetFile;
            RedirectStandardOutput = true;
            FileName = "nuget";
        }

        protected RoboNuGetFile RoboNuGetFile { get; }

        // Gets NuGet command-name.
        protected abstract string Name { get; }

        protected string Command => RoboNuGetFile.NuGet.Commands[Name];
    }
}