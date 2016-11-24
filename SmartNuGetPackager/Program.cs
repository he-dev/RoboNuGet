using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Reusable;
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
            var packageNuspecs = GetPackageNuspecs(Path.GetDirectoryName(config.MsBuild.ActualProjectFile));

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