using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core;
using Autofac.Core.Activators.Delegate;
using Autofac.Core.Activators.Reflection;
using Autofac.Core.Lifetime;
using Autofac.Core.Registration;
using JetBrains.Annotations;
using Reusable;
using Reusable.Commander;
using Reusable.Commander.Utilities;
using Reusable.Extensions;
using Reusable.IO;
using Reusable.OmniLog;
using RoboNuGet.Commands;
using RoboNuGet.Files;

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

                    try
                    {
                        if (commandLine.IsNullOrEmpty())
                        {
                            logger.ConsoleError("Invalid command name.");
                        }
                        else
                        {
                            await executor.ExecuteAsync(commandLine);
                        }
                    }
                    catch (Exception exception)
                    {
                        logger.ConsoleException(exception);
                    }
                } while (true);
            }

            // ReSharper disable once FunctionNeverReturns - it does return when you execute the 'exit' command
        }

        private static IContainer InitializeContainer(RoboNuGetFile configuration, ILoggerFactory loggerFactory)
        {
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
                .RegisterGeneric(typeof(Logger<>))
                .As(typeof(ILogger<>));

            builder
                .RegisterModule(
                    new CommanderModule(commands =>
                        commands
                            .Add<Commands.UpdateNuspec>()
                            .Add<Commands.Version>()
                            .Add<Commands.Clear>()
                            .Add<Commands.Build>()
                            .Add<Commands.Pack>()
                            .Add<Commands.List>()
                            .Add<Commands.Push>()
                            .Add<Commands.Exit>()
                            .Add<Reusable.Commander.Commands.Help>()
                    )
                );

            builder.RegisterSource(new TypeListSource<IConsoleCommand>());

            return builder.Build();
        }
    }

    public static class ExitCode
    {
        public const int Success = 0;
    }    
}