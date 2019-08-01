using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Reusable;
using Reusable.Commander;
using Reusable.Commander.DependencyInjection;
using Reusable.Extensions;
using Reusable.IOnymous;
using Reusable.OmniLog;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Console;
using t = RoboNuGet.ConsoleTemplates;
using RoboNuGet.Files;
using RoboNuGet.Services;

namespace RoboNuGet
{
    internal class Program
    {
        //public static readonly ConsoleStyle Style = new ConsoleStyle(ConsoleColor.Black, ConsoleColor.Gray);
        
        private static async Task Main(string[] args)
        {
            var configuration = RoboNuGetFile.Load();
            var loggerFactory = new LoggerFactory
            {
                Observers =
                {
                    new ConsoleRx(new ConsoleRenderer())
                    {
                        //Style = new ConsoleStyle(ConsoleColor.Blue, ConsoleColor.Cyan)
                    }
                    //ConsoleTemplateRx.Create(new ConsoleTemplateRenderer())
                }
            };

            using (var container = InitializeContainer(configuration, loggerFactory))
            using (var scope = container.BeginLifetimeScope())
            {
                var logger = scope.Resolve<ILogger<Program>>();
                var executor = scope.Resolve<ICommandExecutor>();
                var commandFactory = scope.Resolve<ICommandFactory>();

                // main loop

                await executor.ExecuteAsync("cls", default(object), commandFactory, CancellationToken.None);

                do
                {
                    logger.Write(default, new t.Prompt());
                    var commandLine = System.Console.ReadLine();

                    try
                    {
                        if (commandLine.IsNullOrEmpty())
                        {
                            logger.WriteLine(default, new t.Error { Text = "Invalid command name" });
                        }
                        else
                        {
                            await executor.ExecuteAsync<object>(commandLine, default, commandFactory);
                        }
                    }
                    catch (Exception exception)
                    {
                        //logger.ConsoleException(exception);
                        foreach (var (ex, _) in exception.SelectMany())
                        {
                            logger.WriteLine(default, new t.Error { Text = $"{ex.GetType().Name}: {ex.Message}" });
                        }
                    }
                } while (true);
            }

            // ReSharper disable once FunctionNeverReturns - it does return when you execute the 'exit' command
        }

        private static IContainer InitializeContainer(RoboNuGetFile roboNuGetFile, ILoggerFactory loggerFactory)
        {
            var builder = new ContainerBuilder();

            builder
                .RegisterInstance(roboNuGetFile);

            builder
                .RegisterType<ProcessExecutor>()
                .As<IProcessExecutor>();

            builder
                .RegisterType<PhysicalDirectoryTree>()
                .As<IDirectoryTree>();

            builder
                .RegisterType<SolutionDirectoryTree>();

            builder
                .RegisterInstance(loggerFactory)
                .As<ILoggerFactory>();

            builder
                .RegisterGeneric(typeof(Logger<>))
                .As(typeof(ILogger<>));

            var commands =
                ImmutableList<CommandModule>
                    .Empty
                    .Add<Commands.UpdateNuspec>()
                    .Add<Commands.Version>()
                    .Add<Commands.Clear>()
                    .Add<Commands.Select>()
                    .Add<Commands.Build>()
                    .Add<Commands.Pack>()
                    .Add<Commands.List>()
                    .Add<Commands.Push>()
                    .Add<Commands.Exit>()
                    .Add<Reusable.Commander.Commands.Help>();
            builder
                .RegisterModule(new CommanderModule(commands));

//            builder
//                .Register(ctx =>
//                {
//                    var logger = ctx.Resolve<ILogger<Program>>();
//                    return (ExecuteExceptionCallback)(exception => { logger.ConsoleException(exception); });
//                })
//                .SingleInstance();

            return builder.Build();
        }
    }

    public static class ExitCode
    {
        public const int Success = 0;
    }

    public static class ProgramInfo
    {
        public const string Name = "RoboNuGet";
        
        public const string Version = "6.0.3";
    }
}