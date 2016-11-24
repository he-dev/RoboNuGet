using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using Newtonsoft.Json;
using Reusable;
using RoboNuGet.Commands;

namespace RoboNuGet
{
    using Data;

    internal class Program
    {
        internal Config Config { get; }

        internal IEnumerable<PackageNuspec> PackageNuspecs { get; }

        internal bool AutoIncrementPatchVersion { get; set; }

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
            var menu = new Menu(this)
            {
                Execute = ExecuteCommand
            };

            menu.Start();
        }

        private bool ExecuteCommand(string command)
        {
            var commandParts = command.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
            var commandName = commandParts.ElementAt(0);
            var commandArg = commandParts.ElementAtOrDefault(1);

            if (commandName.Equals(BuildCommand.Name, StringComparison.OrdinalIgnoreCase))
            {
                return new BuildCommand
                {
                    MsBuild = Config.MsBuild
                }
                .Execute();
            }

            // todo: move to a new multipack command
            if (commandName.Equals(PackCommand.Name, StringComparison.OrdinalIgnoreCase))
            {
                if (AutoIncrementPatchVersion)
                {
                    Config.IncrementPatchVersion();
                    Config.Save();
                }

                foreach (var packageNuspec in PackageNuspecs)
                {
                    var packResult = new PackCommand
                    {
                        PackageNuspec = packageNuspec,
                        Version = Config.FullVersion,
                        Outputdirectory = Config.PackageDirectoryName,
                    }.Execute();
                }


                ConsoleColorizer.Render($"<text>&gt;<color fg=\"darkgray\">---</color></text>");

                var all = packResults.All(x => x);

                ConsoleColorizer.Render(all
                    ? $"<text>&gt;<color fg=\"green\">All packages successfuly created.</color> <color fg=\"darkyellow\">(Press Enter to continue)</color></text>"
                    : $"<text>&gt;<color fg=\"green\">Some packages could not be created.</color> <color fg=\"darkyellow\">(Press Enter to continue)</color></text>");
                Console.ReadKey();

                return all;
            }

            if (commandName.Equals("push", StringComparison.OrdinalIgnoreCase))
            {
                var pushResults = PackageNuspecs.Select(packageNuspec => new PushCommand
                {
                    NuGetConfigFileName = Config.NuGetConfigName,
                    PackageId = packageNuspec.Id,
                    PackagesDirectoryName = Config.PackageDirectoryName,
                    Version = Config.FullVersion
                }
                .Execute())
                .ToList();
                return pushResults.All(x => x);
            }

            if (commandName.Equals(".autover", StringComparison.OrdinalIgnoreCase))
            {
                AutoIncrementPatchVersion = commandArg == null || bool.Parse(commandArg);
                return true;
            }

            if (commandName.Equals(".version", StringComparison.OrdinalIgnoreCase))
            {
                // todo: needs validation
                Config.PackageVersion = commandArg;
                Config.Save();
            }

            if (commandName.Equals(".exit", StringComparison.OrdinalIgnoreCase))
            {
                Environment.Exit(0);
            }

            return false;
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

namespace RoboNuGet.Data
{
    internal class Config
    {
        private const string DefaultFileName = "RoboNuGet.json";

        [JsonIgnore]
        public string FileName { get; private set; }

        public string PackageDirectoryName { get; set; }

        public string NuGetConfigName { get; set; }

        public string PackageVersion { get; set; }

        public string[] TargetFrameworkVersions { get; set; }

        [JsonIgnore]
        public string FullVersion => IsPrerelease ? $"{PackageVersion}-pre" : PackageVersion;

        public bool IsPrerelease { get; set; }

        public MsBuild MsBuild { get; set; }

        public string[] NuGet { get; set; }

        public static Config Load()
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            var fileName = Path.Combine(currentDirectory, DefaultFileName);

            var json = File.ReadAllText(fileName);
            var config = JsonConvert.DeserializeObject<Config>(json);
            config.FileName = fileName;
            return config;
        }

        public void Save()
        {
            var json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(FileName, json);
        }

        public void IncrementPatchVersion()
        {
            PackageVersion = Regex.Replace(PackageVersion, @"^(?<major>\d+)\.(?<minor>\d+)\.(?<patch>\d+)", m =>
                $"{m.Groups["major"].Value}." +
                $"{m.Groups["minor"]}." +
                $"{int.Parse(m.Groups["patch"].Value) + 1}");
        }
    }

    internal class MsBuild
    {
        private readonly Lazy<string> _lazyProjectFile = new Lazy<string>(FindSolutionFileName);

        public string Target { get; set; }

        public bool NoLogo { get; set; }

        public Dictionary<string, string> Properties { get; set; }

        public string ProjectFile { get; set; }

        [JsonIgnore]
        public string ActualProjectFile => string.IsNullOrEmpty(ProjectFile) ? _lazyProjectFile.Value : ProjectFile;

        private static string FindSolutionFileName()
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

        public override string ToString()
        {
            var arguments = new List<string>();

            if (!string.IsNullOrEmpty(Target))
            {
                arguments.Add($"/target:{Target}");
            }

            if (NoLogo)
            {
                arguments.Add("/nologo");
            }

            arguments.AddRange(Properties.Select(property => $"/property:{property.Key}=\"{property.Value}\""));

            arguments.Add(ActualProjectFile);

            return string.Join(" ", arguments);
        }

        public static implicit operator string(MsBuild msBuild)
        {
            return msBuild.ToString();
        }
    }

    internal class PackageNuspec
    {
        private readonly XDocument _packageNuspec;

        public PackageNuspec(string fileName)
        {
            _packageNuspec = XDocument.Load(FileName = fileName);
            RemoveDependencies();
        }

        public static PackageNuspec From(string dirName)
        {
            var packageNuspecFileName = Directory.GetFiles(dirName, "*.nuspec").SingleOrDefault();
            return string.IsNullOrEmpty(packageNuspecFileName) ? null : new PackageNuspec(packageNuspecFileName);
        }

        public string FileName { get; }

        public string Id
        {
            get
            {
                var xId = ((IEnumerable)_packageNuspec.XPathEvaluate(@"package/metadata/id")).Cast<XElement>().Single();
                return xId.Value;
            }
        }

        public string Version
        {
            get
            {
                var xVersion = ((IEnumerable)_packageNuspec.XPathEvaluate(@"package/metadata/version")).Cast<XElement>().Single();
                return xVersion.Value;
            }
            set
            {
                var xVersion = ((IEnumerable)_packageNuspec.XPathEvaluate(@"package/metadata/version")).Cast<XElement>().Single();
                xVersion.Value = value;
            }
        }

        public void AddDependency(string id, string version)
        {
            var xDependencies = ((IEnumerable)_packageNuspec.XPathEvaluate(@"package/metadata/dependencies")).Cast<XElement>().SingleOrDefault();
            if (xDependencies == null)
            {
                var xMetadata = ((IEnumerable)_packageNuspec.XPathEvaluate(@"package/metadata")).Cast<XElement>().Single();
                xMetadata.Add(xDependencies = new XElement("dependencies"));
            }
            xDependencies.Add(new XElement("dependency", new XAttribute("id", id), new XAttribute("version", version)));
        }

        private void RemoveDependencies()
        {
            var xDependencies = ((IEnumerable)_packageNuspec.XPathEvaluate(@"package/metadata/dependencies")).Cast<XElement>().SingleOrDefault();
            xDependencies?.Remove();
        }

        public void Save()
        {
            _packageNuspec.Save(FileName, SaveOptions.None);
        }
    }

    internal class PackagesConfig
    {
        private PackagesConfig(string fileName)
        {
            if (!File.Exists(fileName))
            {
                Packages = Enumerable.Empty<PackageElement>();
                return;
            }

            var packagesConfig = XDocument.Load(FileName = fileName);

            var packages = ((IEnumerable)packagesConfig.XPathEvaluate(@"packages/package"))?.Cast<XElement>();
            Packages = packages.Select(x => new PackageElement
            {
                Id = x.Attribute("id").Value,
                Version = x.Attribute("version").Value
            }).ToList();
        }

        private string FileName { get; }

        public IEnumerable<PackageElement> Packages { get; }

        public static PackagesConfig From(string dirName)
        {
            var packagesConfigFileName = Path.Combine(dirName, "packages.config");
            return new PackagesConfig(packagesConfigFileName);
        }

        internal class PackageElement
        {
            public string Id { get; set; }
            public string Version { get; set; }
        }
    }

    internal class CsProj
    {
        private CsProj(IEnumerable<string> projectReferenceNames)
        {
            ProjectReferenceNames = projectReferenceNames;
        }

        public IEnumerable<string> ProjectReferenceNames { get; }

        public static CsProj From(string dirName)
        {
            var csprojFileName = Directory.GetFiles(dirName, "*.csproj").Single();
            var csproj = XDocument.Load(csprojFileName);
            var projectReferenceNames =
                ((IEnumerable)csproj.XPathEvaluate("//*[contains(local-name(), 'ProjectReference')]"))
                .Cast<XElement>()
                .Select(x => x.Element(XName.Get("Name", csproj.Root.GetDefaultNamespace().NamespaceName)).Value)
                .ToList();

            return new CsProj(projectReferenceNames);
        }
    }
}
