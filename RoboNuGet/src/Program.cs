﻿using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Reusable;
using Reusable.Commander;
using Reusable.Commander.Commands;
using Reusable.Commander.DependencyInjection;
using Reusable.Extensions;
using Reusable.IOnymous;
using Reusable.OmniLog;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;
using Reusable.OmniLog.Extensions;
using Reusable.OmniLog.Nodes;
using Reusable.OmniLog.Rx;
using Reusable.OmniLog.Rx.ConsoleRenderers;
using Reusable.OmniLog.SemanticExtensions.Nodes;
using Reusable.OmniLog.Utilities;
using t = RoboNuGet.ConsoleTemplates;
using RoboNuGet.Files;
using RoboNuGet.Services;

namespace RoboNuGet
{
    internal class Program
    {
        public static readonly ConsoleStyle Style = new ConsoleStyle(ConsoleColor.Black, ConsoleColor.Gray);

        private static async Task Main(string[] args)
        {
            var configuration = RoboNuGetFile.Load();
            var loggerFactory = new LoggerFactory
            {
                Nodes =
                {
                    new ConstantNode
                    {
                        { "Environment", "Demo" },
                        { "Product", "Reusable.app.Console" }
                    },
                    new StopwatchNode
                    {
                        GetValue = elapsed => elapsed.TotalMilliseconds
                    },
                    new ComputableNode
                    {
                        Computables =
                        {
                            new Reusable.OmniLog.Computables.Timestamp<DateTimeUtc>()
                        }
                    },
                    new LambdaNode(),
                    //new CorrelationNode(),
                    //new SemanticNode(),
                    //new DumpNode(),
                    //new SerializationNode(),
                    //new FilterNode(logEntry => true),
                    // Renames properties.
//                    new RenameNode
//                    {
//                        Changes =
//                        {
//                            { CorrelationNode.DefaultLogEntryItemNames.Scope, "Scope" },
//                            { DumpNode.DefaultLogEntryItemNames.Variable, "Identifier" },
//                            { DumpNode.DefaultLogEntryItemNames.Dump, "Snapshot" },
//                        }
//                    },
//                    new FallbackNode
//                    {
//                        Defaults =
//                        {
//                            [LogEntry.BasicPropertyNames.Level] = LogLevel.Information
//                        }
//                    },
                    //new TransactionNode(),
                    new EchoNode
                    {
                        Rx =
                        {
                            //new NLogRx(),
                            new ConsoleRx // Use console.
                            {
                                Renderer = new HtmlConsoleRenderer() { }
                            }
                        },
                    }
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
                    logger.Write(new t.Prompt());
                    var commandLine = System.Console.ReadLine();

                    try
                    {
                        if (commandLine.IsNullOrEmpty())
                        {
                            logger.WriteLine(new t.Error { Text = "Invalid command name" });
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
                            logger.WriteLine(new t.Error { Text = $"{ex.GetType().Name}: {ex.Message}" });
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
                    .Add<Reusable.Commander.Commands.Help>(b => b.WithProperty(nameof(Help.Style), Program.Style));
            builder
                .RegisterModule(new CommanderModule(commands));

            return builder.Build();
        }
    }

    public static class LoggerExtensions
    {
        public static void WriteLine(this ILogger logger, params HtmlConsoleTemplateBuilder[] builders)
        {
            logger.WriteLine(Program.Style, builders);
        }
        
        public static void Write(this ILogger logger, params HtmlConsoleTemplateBuilder[] builders)
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

        public const string Version = "6.0.4";
    }
}