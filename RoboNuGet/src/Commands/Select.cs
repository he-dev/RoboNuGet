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
using RoboNuGet.Files;

namespace RoboNuGet.Commands
{
    internal interface ISelectParameter : ICommandArgumentGroup
    {
        [Description("Solution number (1-based).")]
        [Position(1)]
        int Solution { get; }
    }

    [Description("Select solution.")]
    [Tags("s")]
    internal class Select : Command<ISelectParameter>
    {
        private readonly RoboNuGetFile _roboNuGetFile;

        public Select(CommandServiceProvider<Select> serviceProvider, RoboNuGetFile roboNuGetFile) : base(serviceProvider)
        {
            _roboNuGetFile = roboNuGetFile;
        }

        protected override Task ExecuteAsync(ICommandLineReader<ISelectParameter> parameter, object context, CancellationToken cancellationToken)
        {
            var solution = _roboNuGetFile.Solutions.ElementAtOrDefault(parameter.GetItem(x => x.Solution) - 1);
            if (!(solution is null))
            {
                _roboNuGetFile.SelectedSolution = solution;
            }

            Logger.WriteLine(p => p.Indent(1).text($"Selected {Path.GetFileNameWithoutExtension(_roboNuGetFile.SelectedSolution.FileName)}"));

            return Task.CompletedTask;
        }
    }
}