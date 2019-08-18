using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Reusable.Commander;
using Reusable.Commander.Annotations;
using Reusable.Data.Annotations;
using Reusable.OmniLog.Abstractions;
using t = RoboNuGet.ConsoleTemplates;
using RoboNuGet.Files;

namespace RoboNuGet.Commands
{
    

    [Description("Select solution.")]
    [Tags("s")]
    internal class Select : Command<Select.CommandLine>
    {
        private readonly RoboNuGetFile _roboNuGetFile;

        public Select(ILogger<Select> logger, RoboNuGetFile roboNuGetFile) : base(logger)
        {
            _roboNuGetFile = roboNuGetFile;
        }

        protected override Task ExecuteAsync(CommandLine commandLine, object context, CancellationToken cancellationToken)
        {
            var solution = _roboNuGetFile.Solutions.ElementAtOrDefault(commandLine.Solution - 1);
            if (solution is null)
            {
                Logger.WriteLine(new t.Indent(1), new t.Error { Text = $"Solution {commandLine.Solution} does not exist." });
            }
            else
            {
                _roboNuGetFile.SelectedSolution = solution;

                Logger.WriteLine(new t.Indent(1), new t.Select.Response
                {
                    SolutionName = Path.GetFileNameWithoutExtension(_roboNuGetFile.SelectedSolution.FileName)
                });
            }


            return Task.CompletedTask;
        }
        
        internal class CommandLine : CommandLineBase
        {
            public CommandLine(CommandLineDictionary arguments) : base(arguments) { }

            [Description("Solution number (1-based).")]
            [Position(1)]
            public int Solution => GetArgument(() => Solution);
        }
    }
}