using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using Newtonsoft.Json;

namespace SmartNuGetPackager
{
    using Commands;
    using Data;

    internal class Program
    {
        private static Config Config { get; set; }

        private static void Main(string[] args)
        {
            Config = Config.Load();
            var menu = new Menu(Config)
            {
                Execute = Execute
            };
            menu.Start();
            Console.WriteLine("Done. (Press any key to exit)");
            Console.ReadKey();
        }

        private static bool Execute(string commandName)
        {
            if (commandName.Equals(BuildCommand.Name, StringComparison.OrdinalIgnoreCase))
            {
                return new BuildCommand
                {
                    MsBuild = Config.MsBuild
                }
                .Execute();
            }

            if (commandName.StartsWith(PackCommand.Name, StringComparison.OrdinalIgnoreCase))
            {
                var packageNuspecs = GetNuspecs();

                if (commandName.EndsWith(":autover"))
                {
                    Config.Version = Config.Version.IncreasePatchVersion();
                    Config.Save();
                }                

                return UpdateNuspecs(packageNuspecs, Config.FullVersion).All(packageNuspec => new PackCommand
                {
                    NuspecFileName = packageNuspec.FileName,
                    Outputdirectory = Config.PackageDirectoryName
                }
                .Execute());
            }

            return false;
        }

        private static IEnumerable<PackageNuspec> GetNuspecs()
        {
            var directories = Directory.GetDirectories(Directory.GetCurrentDirectory());
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

        private static IEnumerable<PackageNuspec> UpdateNuspecs(IEnumerable<PackageNuspec> nuspecs, string packagesVersion)
        {
            foreach (var packageNuspec in nuspecs)
            {
                var directory = Path.GetDirectoryName(packageNuspec.FileName);
                var packagesConfig = PackagesConfig.From(directory);
                var csProj = CsProj.From(directory);

                foreach (var package in packagesConfig.Packages)
                {
                    packageNuspec.AddDependency(package.Id, package.Version);
                }

                foreach (var projectReferenceName in csProj.ProjectReferenceNames)
                {
                    packageNuspec.AddDependency(projectReferenceName, packagesVersion);
                }

                packageNuspec.SetVersion(packagesVersion);
                packageNuspec.Save();

                yield return packageNuspec;
            }
        }
    }

    internal static class PackageVersionExtensions
    {
        public static string IncreasePatchVersion(this string version)
        {
            return Regex.Replace(version, @"^(?<major>\d+)\.(?<minor>\d+)\.(?<patch>\d+)", m =>
                $"{m.Groups["major"].Value}." +
                $"{m.Groups["minor"]}." +
                $"{int.Parse(m.Groups["patch"].Value) + 1}");
        }
    }

    internal class Menu
    {
        private string _lastCommand;

        private readonly string[] _commands =
        {
            "build",
            "pack",
            "push",
            ".exit",
        };

        public Menu(Config config)
        {
            Config = config;
        }

        public Config Config { get; }

        public Func<string, bool> Execute { get; set; }

        public void Start()
        {
            var command = string.Empty;
            do
            {
                Console.Clear();
                Console.WriteLine($"Project: {Path.GetFileNameWithoutExtension(Config.MsBuild.CurrentProjectFile)}");
                Console.WriteLine($"What do you want to do: {string.Join(", ", _commands)}?");
                if (!string.IsNullOrEmpty(_lastCommand))
                {
                    Console.WriteLine($"Last command: {_lastCommand} (Press Enter to use)");
                }
                Console.Write(">");
                command = Console.ReadLine();
                if (string.IsNullOrEmpty(command))
                {
                    command = _lastCommand;
                }
                if (string.IsNullOrEmpty(command))
                {
                    Console.WriteLine("Command must not be empty. (Press Enter to continue)");
                }

                if (command.Trim().Equals(".exit", StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }

                _lastCommand = command;

                var commands = command.Split(' ');
                foreach (var cmd in commands)
                {
                    if (!Execute(cmd))
                    {
                        break;
                    }
                }

            } while (true);
        }
    }
}

namespace SmartNuGetPackager.Commands
{
    using Data;

    internal abstract class Command
    {
        public abstract bool Execute();
    }

    internal abstract class StartProcessCommand : Command
    {
        protected bool Execute(string fileName, string arguments)
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "cmd",
                Arguments = $"/Q /C pause | {fileName} {arguments}",
            };
            var process = Process.Start(processStartInfo);
            process.WaitForExit();
            return process.ExitCode == 0;
        }
    }

    internal class BuildCommand : StartProcessCommand
    {
        public const string Name = "build";

        public MsBuild MsBuild { get; set; }

        public override bool Execute()
        {
            return Execute("msbuild", MsBuild.ToString());
        }        
    }

    internal class PackCommand : StartProcessCommand
    {
        public const string Name = "pack";

        public string NuspecFileName { get; set; }

        public string Outputdirectory { get; set; }

        public override bool Execute()
        {
            return Execute("nuget", CreatePackCommand());
        }

        private string CreatePackCommand()
        {
            return
                $"pack " +
                $"\"{NuspecFileName}\" " +
                $"-properties Configuration=Release " +
                $"-outputdirectory {Outputdirectory}";
        }
    }

    internal class PushCommand : StartProcessCommand
    {
        public const string Name = "push";

        public string PackagesDirectoryName { get; set; }

        public string PackageId { get; set; }

        public string Version { get; set; }

        public string NuGetConfigFileName { get; set; }

        public override bool Execute()
        {
            return Execute("nuget", CreatePushCommand());
        }

        private string CreatePushCommand()
        {
            var nupkgFileName = $"{Path.Combine(PackagesDirectoryName, $"{PackageId}.{Version}.nupkg")}";
            return
                $"push " +
                $"\"{nupkgFileName}\" " +
                $"-configfile {NuGetConfigFileName}";
        }
    }
}

namespace SmartNuGetPackager.Data
{
    internal class Config
    {
        private const string DefaultFileName = "SmartNuGetPackager.json";

        [JsonIgnore]
        public string FileName { get; private set; }

        public string PackageDirectoryName { get; set; }

        public string NuGetConfigName { get; set; }

        public string Version { get; set; }

        [JsonIgnore]
        public string FullVersion => IsPrerelease ? $"{Version}-pre" : Version;

        public bool IsPrerelease { get; set; }

        public MsBuild MsBuild { get; set; }

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
    }

    internal class MsBuild
    {
        public string Target { get; set; }

        public bool NoLogo { get; set; }

        public Dictionary<string, string> Properties { get; set; }

        public string ProjectFile { get; set; }

        [JsonIgnore]
        public string CurrentProjectFile
        {
            get
            {
                if (!string.IsNullOrEmpty(ProjectFile))
                {
                    return ProjectFile;
                }

                var sln = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.sln").SingleOrDefault();
                if (string.IsNullOrEmpty(sln))
                {
                    throw new InvalidOperationException($"Solution file not found in \"{Directory.GetCurrentDirectory()}\".");
                }

                return sln;
            }
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

            foreach (var property in Properties)
            {
                arguments.Add($"/property:{property.Key}=\"{property.Value}\"");
            }

            arguments.Add(ProjectFile);

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
            return packageNuspecFileName == null ? null : new PackageNuspec(packageNuspecFileName);
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

        public void SetVersion(string id)
        {
            var xVersion = ((IEnumerable)_packageNuspec.XPathEvaluate(@"package/metadata/version")).Cast<XElement>().Single();
            xVersion.Value = id;
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
