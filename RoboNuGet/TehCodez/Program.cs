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
using Reusable.OmniLog;
using RoboNuGet.Commands;
using RoboNuGet.Data;
using Version = RoboNuGet.Commands.Version;

namespace RoboNuGet
{
    internal static class Program
    {
        private static async Task Main(string[] args)
        {
            var configuration = RoboNuGetFile.Load();

            //var solutionDirectoryName = Path.GetDirectoryName(configuration.SolutionFileName);
            //var nuspecFiles = FileFinder.FindNuspecFiles(solutionDirectoryName);

            var loggerFactory = new LoggerFactory();
            var logger = loggerFactory.CreateLogger("ConsoleTemplateTest");
            loggerFactory.Subscribe(ConsoleTemplateRx.Create(new ConsoleTemplateRenderer()));

            var registrations =
                CommandRegistrationContainer
                    .Empty
                    .Register<UpdateNuspec>()
                    .Register<Version>()
                    .Register<Clear>()
                    .Register<Patch>()
                    .Register<Build>()
                    .Register<Pack>()
                    .Register<List>()
                    .Register<Push>()
                    .Register<Exit>();

            var builder = new ContainerBuilder();

            builder
                .RegisterInstance(configuration);

            builder
                .RegisterType<FileService>()
                .As<IFileService>();

            builder
                .RegisterInstance(loggerFactory)
                .As<ILoggerFactory>();

            builder
                .RegisterModule(new CommanderModule(registrations));


            using (var container = builder.Build())
            using (var scope = container.BeginLifetimeScope())
            {
                var executor = scope.Resolve<ICommandLineExecutor>();

                // main loop

                await executor.ExecuteAsync("cls", CancellationToken.None);

                do
                {
                    logger.ConsoleSpan(null, null, s => s.Prompt());
                    var commandLine = Console.ReadLine();

                    if (commandLine.IsNullOrEmpty())
                    {
                        logger.ConsoleParagraph(p => p.ConsoleSpan(ConsoleColor.Red, null, _ => "Invalid command name."));
                        continue;
                    }

                    try
                    {
                        await executor.ExecuteAsync(commandLine, CancellationToken.None);
                    }
                    catch (DynamicException exception) when (exception.NameEquals("CommandNotFoundException"))
                    {
                        logger.ConsoleParagraph(p => p.ConsoleSpan(ConsoleColor.Red, null, _ => "Invalid command name."));
                    }
                } while (true);
            }
            // ReSharper disable once FunctionNeverReturns
        }
    }
}