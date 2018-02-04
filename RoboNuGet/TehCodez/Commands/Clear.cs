using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Commander;
using Reusable.ConsoleColorizer;
using Reusable.Extensions;
using Reusable.MarkupBuilder.Html;
using Reusable.OmniLog;
using RoboNuGet.Files;

namespace RoboNuGet.Commands
{
    [UsedImplicitly]
    [Alias("cls")]
    internal class Clear : ConsoleCommand
    {
        private readonly RoboNuGetFile _roboNuGetFile;
        private readonly IFileSearch _fileSearch;

        public Clear(ILoggerFactory loggerFactory, RoboNuGetFile roboNuGetFile, IFileSearch fileSearch) : base(loggerFactory)
        {
            _roboNuGetFile = roboNuGetFile;
            _fileSearch = fileSearch;
        }

        public override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            Console.Clear();
            RenderSplashScreen(_roboNuGetFile);
            return Task.CompletedTask;
        }

        private void RenderSplashScreen(RoboNuGetFile roboNuGetFile)
        {
            Logger.ConsoleMessageLine(m => m.Prompt().span(s => s.text("RoboNuGet v4.0.0").color(ConsoleColor.DarkGray)));

            var solutionFileName = _fileSearch.FindSolutionFile();
            var nuspecFiles = _fileSearch.FindNuspecFiles().ToList();

            //ConsoleColorizer.RenderLine($"<p>&gt;Solution '<span color='yellow'>{solutionName}</span>' <span color='magenta'>v{_program.RoboNuGetFile.FullVersion}</span> ({nuspecFileCount} nuspec{(nuspecFileCount != 1 ? "s" : string.Empty)})</p>");

            Logger.ConsoleMessageLine(p => p
                .Prompt()
                .text("Solution ")
                .span(s => s.text(Path.GetFileNameWithoutExtension(solutionFileName).QuoteWith("'")).color(ConsoleColor.Yellow))
                .text(" ")
                .span(s => s.text($" v{_roboNuGetFile.FullVersion}").color(ConsoleColor.Magenta))
                .text(" ")
                .text($"({nuspecFiles.Count} nuspec{(nuspecFiles.Count != 1 ? "s" : string.Empty)})")
            );
            //            ConsoleColorizer.RenderLine($"<p>&gt;<span color='darkgray'>Directory '{Path.GetDirectoryName(_program.RoboNuGetFile.SolutionFileNameActual)}'</span></p>");
            //            ConsoleColorizer.RenderLine($"<p>&gt;<span color='darkgray'>Packages '{_program.RoboNuGetFile.PackageDirectoryName}'</span></p>");
            //            ConsoleColorizer.RenderLine($"<p>&gt;<span color='darkgray'>Last command '{(string.IsNullOrEmpty(_lastCommandLine) ? "N/A" : _lastCommandLine)}'</span> <span color='darkyellow'>(Press Enter to reuse)</span></p>");
            //            Console.Write(">");
        }
    }
}