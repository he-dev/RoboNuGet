using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Reusable.Commander;
using Reusable.Commander.Annotations;
using Reusable.Data.Annotations;
using Reusable.MarkupBuilder.Html;
using Reusable.OmniLog;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Console;
using t = RoboNuGet.ConsoleTemplates;
using RoboNuGet.Files;

namespace RoboNuGet.Commands
{
    internal class SelectCommandLine : CommandLine
    {
        public SelectCommandLine(CommandLineDictionary arguments) : base(arguments) { }

        [Description("Solution number (1-based).")]
        [Position(1)]
        public int Solution => GetArgument(() => Solution);
    }

    [Description("Select solution.")]
    [Tags("s")]
    internal class Select : Command<SelectCommandLine>
    {
        private readonly RoboNuGetFile _roboNuGetFile;

        public Select(ILogger<Select> logger, RoboNuGetFile roboNuGetFile) : base(logger)
        {
            _roboNuGetFile = roboNuGetFile;
        }

        protected override Task ExecuteAsync(SelectCommandLine commandLine, object context, CancellationToken cancellationToken)
        {
            var solution = _roboNuGetFile.Solutions.ElementAtOrDefault(commandLine.Solution - 1);
            if (!(solution is null))
            {
                _roboNuGetFile.SelectedSolution = solution;
            }

            Logger.WriteLine(Program.Style, new t.Indent(1), new t.Select.Response
            {
                SolutionName = Path.GetFileNameWithoutExtension(_roboNuGetFile.SelectedSolution.FileName)
            });

            return Task.CompletedTask;
        }
    }
}