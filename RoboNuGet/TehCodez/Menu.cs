using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Reusable;

namespace RoboNuGet
{
    internal class Menu
    {
        private readonly Program _program;

        private string _lastCommandLine;

        private readonly Dictionary<string, Action<string[]>> _commands;

        public Menu(Program program)
        {
            _program = program;

            _commands = new Dictionary<string, Action<string[]>>(StringComparer.OrdinalIgnoreCase)
            {
                ["build"] = _program.Build,
                ["patch"] = _program.Patch,
                ["pack"] = _program.Pack,
                ["push"] = _program.Push,
                ["version"] = _program.Version,
                ["List"] = _program.List,
            };
        }

        public void Start()
        {
            do
            {
                Console.Clear();

                RenderHeader();

                var commandLine = Console.ReadLine();

                if (string.IsNullOrEmpty(commandLine))
                {
                    commandLine = _lastCommandLine;
                }

                if (string.IsNullOrEmpty(commandLine))
                {
                    Picasso.WriteError("Command must not be empty");
                    continue;
                }

                try
                {
                    InvokeCommandLine(commandLine);
                    _lastCommandLine = commandLine;
                }
                catch (Exception ex)
                {
                    Picasso.WriteError(ex.Message);
                }

            } while (true);
            // ReSharper disable once FunctionNeverReturns
        }

        private void RenderHeader()
        {
            ConsoleColorizer.RenderLine($"<p>&gt;<span color='darkgray'>RoboNuGet v3.0.0</span></p>");

            if (string.IsNullOrEmpty(_program.RoboNuGetFile.SolutionFileNameActual))
            {
                Picasso.WriteError("Solution file not found.");
                return;
            }

            var solutionName = Path.GetFileNameWithoutExtension(_program.RoboNuGetFile.SolutionFileNameActual);
            var nuspecFileCount = _program.PackageNuspecs.Count();
            ConsoleColorizer.RenderLine($"<p>&gt;Solution '<span color='yellow'>{solutionName}</span>' <span color='magenta'>v{_program.RoboNuGetFile.FullVersion}</span> ({nuspecFileCount} nuspec{(nuspecFileCount != 1 ? "s" : string.Empty)})</p>");
            ConsoleColorizer.RenderLine($"<p>&gt;<span color='darkgray'>Directory '{Path.GetDirectoryName(_program.RoboNuGetFile.SolutionFileNameActual)}'</span></p>");
            ConsoleColorizer.RenderLine($"<p>&gt;<span color='darkgray'>Packages '{_program.RoboNuGetFile.PackageDirectoryName}'</span></p>");
            ConsoleColorizer.RenderLine($"<p>&gt;<span color='darkgray'>Last command '{(string.IsNullOrEmpty(_lastCommandLine) ? "N/A" : _lastCommandLine)}'</span> <span color='darkyellow'>(Press Enter to reuse)</span></p>");
            Console.Write(">");
        }

        private void InvokeCommandLine(string commandLine)
        {
            var actions = new List<Action>();

            var expressions = commandLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var expression in expressions)
            {
                var subExpressions = expression.Split(':');
                var commandName = subExpressions.ElementAt(0);
                var arguments = subExpressions.ElementAtOrDefault(1);

                var command = (Action<string[]>)null;
                if (!_commands.TryGetValue(commandName, out command))
                {
                    throw new Exception($"\"{expression}\" is not a valid command.");
                }

                actions.Add(() => { command(new[] { arguments }); });
            }

            if (!actions.Any())
            {
                throw new Exception("Invalid command name");
            }

            foreach (var action in actions)
            {
                action();
            }
        }
    }
}