using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using JetBrains.Annotations;
using Reusable.Commander;
using Reusable.ConsoleColorizer;
using Reusable.OmniLog;
using RoboNuGet.Data;
using RoboNuGet.Files;

namespace RoboNuGet.Commands
{
    internal class List : ConsoleCommand
    {
        private readonly IEnumerable<NuspecFile> _packageNuspecs;

        public List(ILoggerFactory loggerFactory, IEnumerable<NuspecFile> packageNuspecs) : base(loggerFactory)
        {
            _packageNuspecs = packageNuspecs;
        }

        public override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            foreach (var packageNuspec in _packageNuspecs)
            {
                Logger.ConsoleParagraph(p => { });
                Logger.ConsoleParagraph(p => p.ConsoleText($"{Path.GetFileNameWithoutExtension(packageNuspec.FileName)} ({packageNuspec.Dependencies.Count()})"));

                foreach (var nuspecDependency in packageNuspec.Dependencies)
                {
                    Logger.ConsoleParagraph(p => p.ConsoleText($"- {nuspecDependency.Id} v{nuspecDependency.Version}"));
                }
            }

            Console.ReadKey();

            return Task.CompletedTask;
        }
    }
}