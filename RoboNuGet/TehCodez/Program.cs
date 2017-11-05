using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Reusable;
using Reusable.Commander;
using Reusable.ConsoleColorizer;
using Reusable.Extensions;
using Reusable.OmniLog;
using RoboNuGet.Commands;
using RoboNuGet.Data;
using RoboNuGet.Files;
using Version = RoboNuGet.Commands.Version;

namespace RoboNuGet
{
    internal class Program
    {
        internal RoboNuGetFile RoboNuGetFile { get; }

        internal IEnumerable<NuspecFile> PackageNuspecs { get; }

        private Program(RoboNuGetFile roboNuGetFile, IEnumerable<NuspecFile> packageNuspecs)
        {
            RoboNuGetFile = roboNuGetFile;
            PackageNuspecs = packageNuspecs;
        }

        private static async Task Main(string[] args)
        {
            var configuration = RoboNuGetFile.Load();

            if (configuration.SolutionFileName.Current.IsNullOrEmpty())
            {
                configuration.SolutionFileName.Value = FileFinder.FindSolutionFileName();
            }

            var solutionDirectoryName = Path.GetDirectoryName(configuration.SolutionFileName);
            var nuspecFiles = FileFinder.FindNuspecFiles(solutionDirectoryName);

            var loggerFactory = new LoggerFactory();
            var logger = loggerFactory.CreateLogger("ConsoleTemplateTest");
            loggerFactory.Subscribe(ConsoleTemplateRx.Create(new ConsoleTemplateRenderer()));

            var registrations =
                CommandRegistrationContainer
                    .Empty
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
                .RegisterInstance(nuspecFiles);

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

                do
                {
                    var commandLine = Console.ReadLine();

                    if (commandLine.IsNullOrEmpty())
                    {
                        logger.ConsoleParagraph(p => p.ConsoleSpan(ConsoleColor.DarkRed, null, _ => "Invalid command name."));
                        continue;
                    }

                    await executor.ExecuteAsync(commandLine, CancellationToken.None);
                } while (true);
            }
            // ReSharper disable once FunctionNeverReturns
        }

        private void Start()
        {
            var menu = new Menu(this);
            menu.Start();
        }

        // --- APIs

        internal void Build(string[] args)
        {
            new Build().Execute(new
            {
                RoboNuGetFile.MsBuild,
                SolutionFile = RoboNuGetFile.SolutionFileNameActual
            });
        }

        internal void Patch(string[] args)
        {
            new Patch().Execute(new
            {
                Config = RoboNuGetFile
            });
        }

        internal void Pack(string[] args)
        {
            var cmd = new Pack(RoboNuGetFile.NuGetCommands).Pre(new UpdateNuspec());

            try
            {
                Parallel.ForEach(PackageNuspecs, packageNuspec =>
                {
                    cmd.Execute(new
                    {
                        PackageNuspec = packageNuspec,
                        PackageVersion = RoboNuGetFile.FullVersion,
                        OutputDirectory = RoboNuGetFile.PackageDirectoryName,
                    });
                });

                ConsoleColorizer.RenderLine($"<p>&gt;<span color='green'>All packages successfuly created.</span> <span color='darkyellow'>(Press Enter to continue)</span></p>");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                ConsoleColorizer.RenderLine($"<p>&gt;<span color='green'>Some packages could not be created.</span> <span color='darkyellow'>(Press Enter to continue)</span></p>");
                Picasso.WriteError(ex.Message);
            }
        }

        internal void Push(string[] args)
        {
            var cmd = new Push(RoboNuGetFile.NuGetCommands);
            foreach (var packageNuspec in PackageNuspecs)
            {
                cmd.Execute(new
                {
                    NuGetConfigFileName = RoboNuGetFile.NuGetConfigName,
                    PackageId = packageNuspec.Id,
                    OutputDirectory = RoboNuGetFile.PackageDirectoryName,
                    FullVersion = RoboNuGetFile.FullVersion,
                });
            }
        }

        internal void Version(string[] args)
        {
            new Version().Execute(new
            {
                Config = RoboNuGetFile,
                Version = args[0]
            });
        }

        internal void List(string[] args)
        {
            new List().Execute(new
            {
                PackageNuspecs
            });
        }
    }


    internal class EmbededAssemblyLoader
    {
        public static void LoadEmbededAssemblies()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                var executingAssembly = Assembly.GetExecutingAssembly();

                var resourceName = $"{new AssemblyName(args.Name).Name}.dll";
                var fullResourceName = executingAssembly.GetManifestResourceNames().SingleOrDefault(x => x.EndsWith(resourceName));
                using (var stream = executingAssembly.GetManifestResourceStream(fullResourceName))
                {
                    if (stream == null)
                    {
                        throw new ApplicationException($"Could not find resource '{fullResourceName}'.");
                    }
                    var assemblyData = new byte[stream.Length];
                    stream.Read(assemblyData, 0, assemblyData.Length);
                    return Assembly.Load(assemblyData);
                }
            };
        }
    }


    internal static class FileFinder
    {
        public static string FindSolutionFileName()
        {
            var directory = Directory.GetParent(Directory.GetCurrentDirectory());
            do
            {
                var files = Directory.GetFiles(directory.FullName, "*.sln");
                if (files.Any())
                {
                    return files.First();
                }
                directory = Directory.GetParent(directory.FullName);
            } while (directory.Parent != null);
            return null;
        }

        public static IEnumerable<NuspecFile> FindNuspecFiles(string solutionDirectoryName)
        {
            var directories = Directory.GetDirectories(solutionDirectoryName);
            foreach (var directory in directories)
            {
                var packageNuspec = NuspecFile.From(directory);
                if (packageNuspec == null)
                {
                    continue;
                }
                yield return packageNuspec;
            }
        }
    }
}