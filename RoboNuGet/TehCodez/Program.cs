using System;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using JetBrains.Annotations;
using Reusable;
using Reusable.Commander;
using Reusable.ConsoleColorizer;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.IO;
using Reusable.OmniLog;
using RoboNuGet.Files;

namespace RoboNuGet
{
    internal static class Program
    {
        private static async Task Main(string[] args)
        {
            var configuration = RoboNuGetFile.Load();
            var loggerFactory = new LoggerFactory(new []
            {
                ConsoleTemplateRx.Create(new ConsoleTemplateRenderer())
            });

            using (var container = InitializeContainer(configuration, loggerFactory))
            using (var scope = container.BeginLifetimeScope())
            {
                var logger = scope.Resolve<ILoggerFactory>().CreateLogger("ConsoleTemplateTest");
                var executor = scope.Resolve<ICommandLineExecutor>();

                // main loop

                await executor.ExecuteAsync("cls", CancellationToken.None);

                do
                {
                    logger.ConsoleMessage(m => m.Prompt());
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
            var registrations =
                CommandRegistrationContainer
                    .Empty
                    .Register<Commands.UpdateNuspec>()
                    .Register<Commands.Version>()
                    .Register<Commands.Clear>()
                    .Register<Commands.Build>()
                    //.Register<Commands.NuGet>()
                    .Register<Commands.Pack>()
                    .Register<Commands.List>()
                    .Register<Commands.Push>()
                    .Register<Commands.Exit>();

            var builder = new ContainerBuilder();

            builder
                .RegisterInstance(configuration);

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
                .RegisterModule(new CommanderModule(registrations));

            return builder.Build();
        }
    }

    public static class ExitCode
    {
        public const int Success = 0;
    }
}