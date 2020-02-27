using System;
using System.Threading.Tasks;
using Autofac;
using Reusable;
using Reusable.Commander;
using Reusable.Commander.Commands;
using Reusable.Commander.DependencyInjection;
using Reusable.Extensions;
using Reusable.IO;
using Reusable.OmniLog;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Helpers;
using Reusable.OmniLog.Services;
using Reusable.OmniLog.Utilities;
using t = RoboNuGet.ConsoleTemplates;
using RoboNuGet.Files;
using RoboNuGet.Services;
using Version = Reusable.Commander.Commands.Version;

namespace RoboNuGet
{
    public class Program
    {
        public static readonly ConsoleStyle Style = new ConsoleStyle(ConsoleColor.Black, ConsoleColor.Gray);

        public static async Task Main()
        {
            using var container = InitializeContainer(InitializeLogger());
            using var scope = container.BeginLifetimeScope();

            var logger = scope.Resolve<ILogger<Program>>();
            var executor = scope.Resolve<ICommandExecutor>();

            var initialCommandLine = "clear -r";

            // main loop
            do
            {
                logger.Write(new t.Prompt());
                var commandLine = initialCommandLine ?? Console.ReadLine();

                try
                {
                    if (commandLine.IsNullOrEmpty())
                    {
                        logger.WriteLine(new t.Error { Text = "Invalid command name" });
                    }
                    else
                    {
                        await executor.ExecuteAsync<object>(commandLine);
                    }
                }
                catch (Exception exception)
                {
                    //logger.ConsoleException(exception);
                    foreach (var (ex, _) in exception.SelectMany())
                    {
                        logger.WriteLine(new t.Error { Text = $"{ex.GetType().Name}: {ex.Message}" });
                    }
                }
                finally
                {
                    initialCommandLine = default;
                }
            } while (true);

            // ReSharper disable once FunctionNeverReturns - it does return when you execute the 'exit' command
        }

        private static IContainer InitializeContainer(ILoggerFactory loggerFactory)
        {
            var builder = new ContainerBuilder();

            builder
                .RegisterType<Session>()
                .SingleInstance();

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

            builder
                .RegisterModule(new CommandModule(m =>
                {
                    m.Register<Commands.UpdateNuspec>();
                    m.Register<Commands.Version>();
                    m.Register<Commands.Clear>();
                    m.Register<Commands.Select>();
                    m.Register<Commands.Build>();
                    m.Register<Commands.Pack>();
                    m.Register<Commands.List>();
                    m.Register<Commands.Push>();
                    m.Register<Commands.Exit>();
                    m.Register<Help>().WithProperty(nameof(Help.Style), Style);
                    //m.Register<Version>().WithParameter("version", ProgramInfo.Version).WithProperty(nameof(Help.Style), Style);
                }));

            return builder.Build();
        }

        private static ILoggerFactory InitializeLogger()
        {
            return
                LoggerFactory
                    .Builder()
                    .UseService
                    (
                        new Constant("Environment", "Demo"),
                        new Constant("Product", "Reusable.app.Console"),
                        new Timestamp<DateTimeUtc>()
                    )
                    .UseStopwatch()
                    .UseDelegate()
                    .UseEcho
                    (
                        new HtmlConsoleRx()
                    )
                    .Build();
        }
    }

    public static class LoggerExtensions
    {
        public static void WriteLine(this ILogger logger, params IHtmlConsoleTemplateBuilder[] builders)
        {
            logger.WriteLine(Program.Style, builders);
        }

        public static void Write(this ILogger logger, params IHtmlConsoleTemplateBuilder[] builders)
        {
            logger.Write(Program.Style, builders);
        }
    }

    public static class ExitCode
    {
        public const int Success = 0;
    }

    public static class ProgramInfo
    {
        public const string Name = "RoboNuGet";
        public const string Version = "7.0.0";
    }

    internal class Session
    {
        public RoboNuGetFile Config { get; set; } = default!;

        /// <summary>
        /// Gets or sets the selected solution.
        /// </summary>
        public Solution? Solution { get; set; }
    }
}