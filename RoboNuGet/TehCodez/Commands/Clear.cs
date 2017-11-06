using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Commander;
using Reusable.CommandLine;
using Reusable.ConsoleColorizer;
using Reusable.Extensions;
using Reusable.OmniLog;
using RoboNuGet.Data;

namespace RoboNuGet.Commands
{
    [UsedImplicitly]
    [Alias("cls")]
    internal class Clear : ConsoleCommand
    {
        private readonly RoboNuGetFile _roboNuGetFile;
        private readonly IFileService _fileService;

        public Clear(ILoggerFactory loggerFactory, RoboNuGetFile roboNuGetFile, IFileService fileService) : base(loggerFactory)
        {
            _roboNuGetFile = roboNuGetFile;
            _fileService = fileService;
        }

        public override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            Console.Clear();
            RenderSplashScreen(_roboNuGetFile);
            return Task.CompletedTask;
        }

        private void RenderSplashScreen(RoboNuGetFile roboNuGetFile)
        {
            Logger.ConsoleParagraph(p => p.Prompt().ConsoleSpan(ConsoleColor.DarkGray, null, s => s.ConsoleText("RoboNuGet v4.0.0")));

            var solutionFileName = roboNuGetFile.GetSolutionFileName(_fileService);
            var nuspecFiles = roboNuGetFile.GetNuspecFiles(_fileService).ToList();

            //            if (string.IsNullOrEmpty(_program.RoboNuGetFile.SolutionFileNameActual))
            //            {
            //                Picasso.WriteError("Solution file not found.");
            //                return;
            //            }

            //ConsoleColorizer.RenderLine($"<p>&gt;Solution '<span color='yellow'>{solutionName}</span>' <span color='magenta'>v{_program.RoboNuGetFile.FullVersion}</span> ({nuspecFileCount} nuspec{(nuspecFileCount != 1 ? "s" : string.Empty)})</p>");

            Logger.ConsoleParagraph(p => p
                .Prompt()
                .ConsoleText("Solution ")
                .ConsoleSpan(ConsoleColor.Yellow, null, s => s.ConsoleText(Path.GetFileNameWithoutExtension(solutionFileName).QuoteWith("'")))
                .ConsoleText(" ")
                .ConsoleSpan(ConsoleColor.Magenta, null, s => s.ConsoleText($" v{_roboNuGetFile.FullVersion}"))
                .ConsoleText(" ")
                .ConsoleText($"({nuspecFiles.Count} nuspec{(nuspecFiles.Count != 1 ? "s" : string.Empty)})")
            );
            //            ConsoleColorizer.RenderLine($"<p>&gt;<span color='darkgray'>Directory '{Path.GetDirectoryName(_program.RoboNuGetFile.SolutionFileNameActual)}'</span></p>");
            //            ConsoleColorizer.RenderLine($"<p>&gt;<span color='darkgray'>Packages '{_program.RoboNuGetFile.PackageDirectoryName}'</span></p>");
            //            ConsoleColorizer.RenderLine($"<p>&gt;<span color='darkgray'>Last command '{(string.IsNullOrEmpty(_lastCommandLine) ? "N/A" : _lastCommandLine)}'</span> <span color='darkyellow'>(Press Enter to reuse)</span></p>");
            //            Console.Write(">");
        }
    }
}