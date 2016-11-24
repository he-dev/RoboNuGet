using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

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
}