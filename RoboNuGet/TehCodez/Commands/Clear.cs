using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Commander;
using Reusable.CommandLine;
using Reusable.OmniLog;
using RoboNuGet.Data;

namespace RoboNuGet.Commands
{
    [UsedImplicitly]
    [Alias("cls")]
    internal class Clear : ConsoleCommand
    {
        private readonly RoboNuGetFile _roboNuGetFile;

        public Clear(ILoggerFactory loggerFactory, RoboNuGetFile roboNuGetFile) : base(loggerFactory)
        {
            _roboNuGetFile = roboNuGetFile;
        }

        public override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            Console.Clear();
            return Task.CompletedTask;
        }
        
        private void RenderSplashScreen()
        {
//            ConsoleColorizer.RenderLine($"<p>&gt;<span color='darkgray'>RoboNuGet v3.0.0</span></p>");
//
//            if (string.IsNullOrEmpty(_program.RoboNuGetFile.SolutionFileNameActual))
//            {
//                Picasso.WriteError("Solution file not found.");
//                return;
//            }
//
//            var solutionName = Path.GetFileNameWithoutExtension(_program.RoboNuGetFile.SolutionFileNameActual);
//            var nuspecFileCount = _program.PackageNuspecs.Count();
//            ConsoleColorizer.RenderLine($"<p>&gt;Solution '<span color='yellow'>{solutionName}</span>' <span color='magenta'>v{_program.RoboNuGetFile.FullVersion}</span> ({nuspecFileCount} nuspec{(nuspecFileCount != 1 ? "s" : string.Empty)})</p>");
//            ConsoleColorizer.RenderLine($"<p>&gt;<span color='darkgray'>Directory '{Path.GetDirectoryName(_program.RoboNuGetFile.SolutionFileNameActual)}'</span></p>");
//            ConsoleColorizer.RenderLine($"<p>&gt;<span color='darkgray'>Packages '{_program.RoboNuGetFile.PackageDirectoryName}'</span></p>");
//            ConsoleColorizer.RenderLine($"<p>&gt;<span color='darkgray'>Last command '{(string.IsNullOrEmpty(_lastCommandLine) ? "N/A" : _lastCommandLine)}'</span> <span color='darkyellow'>(Press Enter to reuse)</span></p>");
//            Console.Write(">");
        }
    }
}