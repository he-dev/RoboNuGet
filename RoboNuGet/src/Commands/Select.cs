using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Reusable.Commander;
using Reusable.Commander.Annotations;
using Reusable.MarkupBuilder.Html;
using Reusable.OmniLog;
using RoboNuGet.Files;

namespace RoboNuGet.Commands
{
    internal class SelectBag : SimpleBag
    {
        [Description("Solution number.")]
        [Position(1)]
        public int Solution { get; set; }
    }

    [Description("Select solution.")]
    internal class Select : ConsoleCommand<SelectBag>
    {
        private readonly RoboNuGetFile _roboNuGetFile;

        public Select(CommandServiceProvider<Select> serviceProvider, RoboNuGetFile roboNuGetFile) : base(serviceProvider)
        {
            _roboNuGetFile = roboNuGetFile;
        }

        protected override Task ExecuteAsync(SelectBag parameter, CancellationToken cancellationToken)
        {
            var solution = _roboNuGetFile.Solutions.ElementAtOrDefault(parameter.Solution);
            if (!(solution is null))
            {
                _roboNuGetFile.SelectedSolution = solution;
            }

            Logger.WriteLine(p => p.Indent(1).text($"Selected {Path.GetFileNameWithoutExtension(_roboNuGetFile.SelectedSolution.FileName)}"));

            return Task.CompletedTask;
        }
    }
}