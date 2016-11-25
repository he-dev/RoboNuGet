using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Reusable;
using Reusable.Commands;
using Reusable.Fuse;
using RoboNuGet.Commands;

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
                ["version"] = _program.Version
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
                    ConsoleTemplates.RenderError("Command must not be empty");
                    continue;
                }

                try
                {
                    InvokeCommandLine(commandLine);
                    _lastCommandLine = commandLine;
                }
                catch (Exception ex)
                {
                    ConsoleTemplates.RenderError(ex.Message);
                }

            } while (true);
            // ReSharper disable once FunctionNeverReturns
        }

        private void RenderHeader()
        {
            ConsoleColorizer.Render($"<p>&gt;<span fg=\"darkgray\">RoboNuGet v2.0.0</span></p>");

            if (string.IsNullOrEmpty(_program.Config.SolutionFileNameActual))
            {
                ConsoleTemplates.RenderError("Solution file not found.");
                return;
            }

            var solutionName = Path.GetFileNameWithoutExtension(_program.Config.SolutionFileNameActual);
            var nuspecFileCount = _program.PackageNuspecs.Count();
            ConsoleColorizer.Render($"<p>&gt;Solution '<span fg=\"yellow\">{solutionName}</span>' <span fg=\"magenta\">v{_program.Config.FullVersion}</span> ({nuspecFileCount} nuspec{(nuspecFileCount != 1 ? "s" : string.Empty)})</p>");
            ConsoleColorizer.Render($"<p>&gt;<span fg=\"darkgray\">Directory '{Path.GetDirectoryName(_program.Config.SolutionFileNameActual)}'</span></p>");
            ConsoleColorizer.Render($"<p>&gt;<span fg=\"darkgray\">Packages '{_program.Config.PackageDirectoryName}'</span></p>");
            ConsoleColorizer.Render($"<p>&gt;<span fg=\"darkgray\">Last command '{(string.IsNullOrEmpty(_lastCommandLine) ? "N/A" : _lastCommandLine)}'</span> <span fg=\"darkyellow\">(Press Enter to reuse)</span></p>");
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