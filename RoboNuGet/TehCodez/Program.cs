using System;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using JetBrains.Annotations;
using Reusable;
using Reusable.Commander;
using Reusable.Extensions;
using Reusable.IO;
using Reusable.OmniLog;
using RoboNuGet.Commands;
using RoboNuGet.Files;
using Version = RoboNuGet.Commands.Version;

namespace RoboNuGet
{
    internal static class Program
    {
        private static async Task Main(string[] args)
        {
            var configuration = RoboNuGetFile.Load();
            var loggerFactory = new LoggerFactory
            {
                Observers =
                {
                    ColoredConsoleRx.Create(new ConsoleRenderer())
                    //ConsoleTemplateRx.Create(new ConsoleTemplateRenderer())
                }
            };

            using (var container = InitializeContainer(configuration, loggerFactory))
            using (var scope = container.BeginLifetimeScope())
            {
                var logger = scope.Resolve<ILoggerFactory>().CreateLogger("ConsoleTemplateTest");
                var executor = scope.Resolve<ICommandLineExecutor>();

                // main loop

                await executor.ExecuteAsync("cls", CancellationToken.None);

                do
                {
                    logger.Write(m => m.Prompt());
                    var commandLine = Console.ReadLine();

                    if (commandLine.IsNullOrEmpty())
                    {
                        //logger.ConsoleParagraph(p => p.ConsoleSpan(ConsoleColor.Red, null, _ => "Invalid command name."));
                        continue;
                    }

                    try
                    {
                        await executor.ExecuteAsync(commandLine, CancellationToken.None);
                    }
                    catch (Exception exception)
                    {
                        logger.ConsoleException(exception);
                    }
                } while (true);
            }
            // ReSharper disable once FunctionNeverReturns - it does renturn when you execute the 'exit' command
        }

        private static IContainer InitializeContainer(RoboNuGetFile configuration, ILoggerFactory loggerFactory)
        {
            var builder = new ContainerBuilder();

            builder
                .RegisterInstance(configuration);

            var commandTypes = new []
            {
                typeof(Commands.UpdateNuspec),
                typeof(Commands.Version),
                typeof(Commands.Clear),
                typeof(Commands.Build),
                typeof(Commands.Pack),
                typeof(Commands.List),
                typeof(Commands.Push),
                typeof(Commands.Exit)
            };

            builder
                .RegisterType<ProcessExecutor>()
                .As<IProcessExecutor>();

            builder
                .RegisterType<FileSystem>()
                .As<IFileSystem>();

            builder
                .RegisterType<FileSearch>()
                .As<IFileSearch>();

            builder
                .RegisterInstance(loggerFactory)
                .As<ILoggerFactory>();

            builder
                .RegisterGeneric(typeof(Logger<>))
                .As(typeof(ILogger<>));

            builder
                .RegisterModule(new CommanderModule(commandTypes));

            return builder.Build();
        }
    }

    public static class ExitCode
    {
        public const int Success = 0;
    }

    internal class Unit { }
}