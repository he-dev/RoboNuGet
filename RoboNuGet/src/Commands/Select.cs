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

namespace RoboNuGet.Commands
{
    [Description("Select solution.")]
    [Alias("s")]
    internal class Select : Command<Select.Parameter>
    {
        private readonly ILogger<Select> _logger;
        private readonly Session _session;

        public Select(ILogger<Select> logger, Session session)
        {
            _logger = logger;
            _session = session;
        }

        protected override Task ExecuteAsync(Parameter parameter, CancellationToken cancellationToken)
        {
            var solution = _session.Config.Solutions.ElementAtOrDefault(parameter.Solution - 1);
            if (solution is null)
            {
                _logger.WriteLine(new t.Indent(1), new t.Error { Text = $"Solution {parameter.Solution} does not exist." });
            }
            else
            {
                _session.Solution = solution;

                _logger.WriteLine(new t.Indent(1), new t.Select.Response
                {
                    SolutionName = Path.GetFileNameWithoutExtension(_session.Solution.FileName)
                });
            }


            return Task.CompletedTask;
        }

        internal class Parameter : CommandParameter
        {
            [Description("Solution number (1-based).")]
            [Position(1)]
            public int Solution { get; set; }
        }
    }
}