using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Commander;
using Reusable.Data.Annotations;
using Reusable.OmniLog.Abstractions;
using t = RoboNuGet.ConsoleTemplates;
using RoboNuGet.Files;
using RoboNuGet.Services;

namespace RoboNuGet.Commands
{
    [Description("Clear the console and refresh package list.")]
    [UsedImplicitly]
    [Alias("cls")]
    internal class Clear : Command<Clear.Parameter>
    {
        private readonly ILogger<Clear> _logger;
        private readonly Session _session;
        private readonly SolutionDirectoryTree _solutionDirectoryTree;

        public Clear
        (
            ILogger<Clear> logger,
            Session session,
            SolutionDirectoryTree solutionDirectoryTree
        )
        {
            _logger = logger;
            _session = session;
            _solutionDirectoryTree = solutionDirectoryTree;
        }

        protected override Task ExecuteAsync(Parameter parameter, CancellationToken cancellationToken)
        {
            System.Console.Clear();

            if (parameter?.Reload == true)
            {
                _session.Config = RoboNuGetFile.Load();
                _session.Solution = default;
            }
            else
            {
                if (parameter?.Solution == true)
                {
                    _session.Solution = default;
                }
            }

            RenderSplashScreen();
            return Task.CompletedTask;
        }

        private void RenderSplashScreen()
        {
            _logger.WriteLine(new t.Prompt(), new t.ProgramInfo());

            if (_session.Solution is null)
            {
                foreach (var (solution, index) in _session.Config.Solutions.Select((s, i) => (s, i + 1)))
                {
                    var nuspecFiles = _solutionDirectoryTree.FindNuspecFiles(solution.DirectoryName).ToList();

                    _logger.WriteLine(new t.Indent(1), new t.Clear.SolutionOption
                    {
                        Index = index,
                        Name = Path.GetFileNameWithoutExtension(solution.FileName),
                        Version = solution.FullVersion,
                        NuspecFileCount = nuspecFiles.Count
                    });
                }

                _logger.WriteLine(new t.Indent(1), new t.Clear.AskForSolution());
            }
            else
            {
                var nuspecFiles = _solutionDirectoryTree.FindNuspecFiles(_session.Solution.DirectoryName).ToList();

                _logger.WriteLine(new t.Indent(1), new t.Clear.SolutionSelection
                {
                    Name = Path.GetFileNameWithoutExtension(_session.Solution.FileName),
                    Version = _session.Solution.FullVersion,
                    NuspecFileCount = nuspecFiles.Count
                });
            }
        }

        internal class Parameter : CommandParameter
        {
            [Description("Reload config.")]
            [Alias("r")]
            public bool Reload { get; set; }

            [Description("Clear selected solution.")]
            [Alias("s")]
            public bool Solution { get; set; }
        }
    }
}