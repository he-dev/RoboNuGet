using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Commander;
using Reusable.Commander.Annotations;
using Reusable.Extensions;
using Reusable.MarkupBuilder.Html;
using Reusable.OmniLog;
using RoboNuGet.Files;

namespace RoboNuGet.Commands
{
    [UsedImplicitly]
    [Alias("cls")]
    internal class Clear : ConsoleCommand<SimpleBag>
    {
        private readonly RoboNuGetFile _roboNuGetFile;
        private readonly IFileSearch _fileSearch;

        public Clear(CommandServiceProvider<Clear> serviceProvider, RoboNuGetFile roboNuGetFile, IFileSearch fileSearch) : base(serviceProvider)
        {
            _roboNuGetFile = roboNuGetFile;
            _fileSearch = fileSearch;
        }

        protected override Task ExecuteAsync(SimpleBag parameter, CancellationToken cancellationToken)
        {
            Console.Clear();
            RenderSplashScreen(_roboNuGetFile);
            return Task.CompletedTask;
        }

        private void RenderSplashScreen(RoboNuGetFile roboNuGetFile)
        {
            Logger.WriteLine(m => m.Prompt().span(s => s.text("RoboNuGet v5.0.0").color(ConsoleColor.DarkGray)));

            var solutionFileName = _fileSearch.FindSolutionFile();
            var nuspecFiles = _fileSearch.FindNuspecFiles().ToList();

            Logger.WriteLine(p => p
                .Prompt()
                .text("Solution ")
                .span(s => s.text(Path.GetFileNameWithoutExtension(solutionFileName).QuoteWith("'")).color(ConsoleColor.Yellow))
                .text(" ")
                .span(s => s.text($"v{_roboNuGetFile.FullVersion}").color(ConsoleColor.Magenta))
                .text(" ")
                .text($"({nuspecFiles.Count} package{(nuspecFiles.Count != 1 ? "s" : string.Empty)})")
            );
        }
    }
}