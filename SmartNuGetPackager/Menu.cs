using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Reusable;
using Reusable.Commands;
using RoboNuGet.Commands;
using RoboNuGet.Data;

namespace RoboNuGet
{
    internal class Menu
    {
        private readonly Program _program;

        private string _lastCommandName;

        private readonly ICommand[] _commands =
        {
            //.exit
            //.autover
            //.version
            new BuildCommand(),
            new PackCommand().Pre(new UpdateNuspecCommand().Pre(new IncrementPathVersionCommand())),
            new PushCommand(),
        };

        public Menu(Program program)
        {
            _program = program;
        }

        public Func<string, bool> Execute { get; set; }

        public void Start()
        {
            do
            {
                Console.Clear();

                ConsoleColorizer.Render($"<text>&gt;<color fg=\"darkgray\">RoboNuGet v2.0.0</color></text>");

                if (string.IsNullOrEmpty(_program.Config.MsBuild.ActualProjectFile))
                {
                    ConsoleColorizer.Render("<text>&gt;<color fg=\"red\">ERROR:</color> Solution file not found.</text>");
                    Console.ReadKey();
                    break;
                }

                var solutionName = Path.GetFileNameWithoutExtension(_program.Config.MsBuild.ActualProjectFile);
                var nuspecFileCount = _program.PackageNuspecs.Count();
                ConsoleColorizer.Render($"<text>&gt;Solution '<color fg=\"yellow\">{solutionName}</color>' <color fg=\"magenta\">v{Program.Config.FullVersion}</color> ({nuspecFileCount} nuspec{(nuspecFileCount != 1 ? "s" : string.Empty)})</text>");
                ConsoleColorizer.Render($"<text>&gt;<color fg=\"darkgray\">.autover '{Program.AutoIncrementPatchVersion}'</color></text>");
                ConsoleColorizer.Render($"<text>&gt;<color fg=\"darkgray\">Last command '{(string.IsNullOrEmpty(_lastCommandName) ? "N/A" : _lastCommandName)}'</color> <color fg=\"darkyellow\">(Press Enter to reuse)</color></text>");
                Console.Write(">");

                var commandLine = Console.ReadLine();

                if (string.IsNullOrEmpty(commandLine))
                {
                    commandLine = _lastCommandName;
                }

                if (string.IsNullOrEmpty(commandLine))
                {
                    ConsoleColorizer.Render($"<text>&gt;<color fg=\"red\">Command must not be empty.</color> <color fg=\"darkyellow\">(Press Enter to continue)</color></text>");
                    Console.Write(">");
                    Console.ReadKey();
                    continue;
                }

                var commandNames = commandLine.Split(' ');

                var commands = _commands
                    .Cast<IIdentifiable>()
                    .Where(x => commandNames.Contains(x.Name, StringComparer.OrdinalIgnoreCase))
                    .ToList();

                if (!commands.Any())
                {
                    ConsoleColorizer.Render($"<text>&gt;<color fg=\"red\">Invalid command name.</color> <color fg=\"darkyellow\">(Press Enter to continue)</color></text>");
                    Console.Write(">");
                    Console.ReadKey();
                    continue;
                }

                ExecuteCommands(commands.Cast<ICommand>());

            } while (true);
            // ReSharper disable once FunctionNeverReturns
        }

        private void ExecuteCommands(IEnumerable<ICommand> commands)
        {
            foreach (var command in commands)
            {
                if (command is BuildCommand)
                {
                    command.Execute(_program.Config.MsBuild);
                    continue;
                }

                if (command is PackCommand)
                {
                    foreach (var packageNuspec in _program.PackageNuspecs)
                    {
                        command.Execute(new
                        {
                            _program.AutoIncrementPatchVersion,
                            _program.Config.FullVersion,
                            _program.Config.PackageDirectoryName,
                            PackageNuspec = packageNuspec,
                        });
                    }
                    continue;
                }

                if (command is PushCommand)
                {
                    continue;
                }
            }
        }
    }
}