using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Reusable;
using Reusable.Commands;
using RoboNuGet.Commands;
using RoboNuGet.Data;

namespace RoboNuGet
{
    internal class Program
    {
        internal Config Config { get; }

        internal IEnumerable<PackageNuspec> PackageNuspecs { get; }

        private Program(Config config, IEnumerable<PackageNuspec> packageNuspecs)
        {
            Config = config;
            PackageNuspecs = packageNuspecs;
        }

        private static void Main(string[] args)
        {
            var config = Config.Load();
            var packageNuspecs = GetPackageNuspecs(Path.GetDirectoryName(config.SolutionFileNameActual));

            new Program(config, packageNuspecs).Start();
        }

        private void Start()
        {
            var menu = new Menu(this);
            menu.Start();
        }

        private static IEnumerable<PackageNuspec> GetPackageNuspecs(string workingDirectory)
        {
            var directories = Directory.GetDirectories(workingDirectory);
            foreach (var directory in directories)
            {
                var packageNuspec = PackageNuspec.From(directory);
                if (packageNuspec == null)
                {
                    continue;
                }
                yield return packageNuspec;
            }
        }

        // --- APIs

        internal void Build(string[] args)
        {
            new BuildCommand().Execute(new
            {
                Config.MsBuild,
                SolutionFile = Config.SolutionFileNameActual
            });
        }

        internal void Patch(string[] args)
        {
            new PatchCommand().Execute(new
            {
                Config = Config
            });
        }

        internal void Pack(string[] args)
        {
            var cmd = new PackCommand(Config.NuGet).Pre(new UpdateNuspecCommand());

            try
            {
                Parallel.ForEach(PackageNuspecs, packageNuspec =>
                {
                    cmd.Execute(new
                    {
                        PackageNuspec = packageNuspec,
                        PackageVersion = Config.FullVersion,
                        OutputDirectory = Config.PackageDirectoryName,
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
            var cmd = new PushCommand(Config.NuGet);
            foreach (var packageNuspec in PackageNuspecs)
            {
                cmd.Execute(new
                {
                    NuGetConfigFileName = Config.NuGetConfigName,
                    PackageId = packageNuspec.Id,
                    OutputDirectory = Config.PackageDirectoryName,
                    FullVersion = Config.FullVersion,
                });
            }
        }

        internal void Version(string[] args)
        {
            new VersionCommand().Execute(new
            {
                Config = Config,
                Version = args[0]
            });
        }

        internal void List(string[] args)
        {
            new ListCommand().Execute(new
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
                    if (stream == null) { throw new ApplicationException($"Could not find resource '{fullResourceName}'."); }
                    var assemblyData = new byte[stream.Length];
                    stream.Read(assemblyData, 0, assemblyData.Length);
                    return Assembly.Load(assemblyData);
                }
            };
        }
    }
}