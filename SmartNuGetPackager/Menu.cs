using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Reusable;
using Reusable.Commands;
using RoboNuGet.Commands;

namespace RoboNuGet
{
    internal class Menu
    {
        private readonly Program _program;

        private string _lastCommandName;

        private readonly ICommand[] _commands;

        public Menu(Program program)
        {
            _program = program;

            _commands = new []
            {
                    //.exit
                    //.autover
                    //.version
                new BuildCommand(),
                new PatchCommand(),
                new PackCommand(_program.Config.NuGet).Pre(new UpdateNuspecCommand()),
                new PushCommand(_program.Config.NuGet),
            };
        }

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
                ConsoleColorizer.Render($"<text>&gt;Solution '<color fg=\"yellow\">{solutionName}</color>' <color fg=\"magenta\">v{_program.Config.FullVersion}</color> ({nuspecFileCount} nuspec{(nuspecFileCount != 1 ? "s" : string.Empty)})</text>");
                //ConsoleColorizer.Render($"<text>&gt;<color fg=\"darkgray\">.autover '{Program.AutoIncrementPatchVersion}'</color></text>");
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
                            PackageNuspec = packageNuspec,
                            FullVersion = _program.Config.FullVersion,
                            OutputDirectory = _program.Config.PackageDirectoryName,
                        });
                    }

                    //ConsoleColorizer.Render($"<text>&gt;<color fg=\"darkgray\">---</color></text>");

                    // ConsoleColorizer.Render(all
                    //? $"<text>&gt;<color fg=\"green\">All packages successfuly created.</color> <color fg=\"darkyellow\">(Press Enter to continue)</color></text>"
                    //: $"<text>&gt;<color fg=\"green\">Some packages could not be created.</color> <color fg=\"darkyellow\">(Press Enter to continue)</color></text>");
                    // Console.ReadKey();

                    continue;
                }

                if (command is PushCommand)
                {
                    foreach (var packageNuspec in _program.PackageNuspecs)
                    {
                        command.Execute(new
                        {
                            NuGetConfigFileName = _program.Config.NuGetConfigName,
                            PackageId = packageNuspec.Id,
                            OutputDirectory = _program.Config.PackageDirectoryName,
                            FullVersion = _program.Config.FullVersion,
                        });
                    }
                    continue;
                }
            }
        }
    }
}