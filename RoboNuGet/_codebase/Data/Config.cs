using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace RoboNuGet.Data
{
    internal class Config
    {
        private const string DefaultFileName = "RoboNuGet.json";

        private readonly Lazy<string> _lazySolutionFileName = new Lazy<string>(FindSolutionFileName);

        public string PackageDirectoryName { get; set; }

        public string NuGetConfigName { get; set; }

        public string SolutionFileName { get; set; }

        public string PackageVersion { get; set; }

        public string[] TargetFrameworkVersions { get; set; }

        public bool IsPrerelease { get; set; }

        public MsBuild MsBuild { get; set; }

        public string[] NuGet { get; set; }

        // Computed properties

        [JsonIgnore]
        public string FullVersion => IsPrerelease ? $"{PackageVersion}-pre" : PackageVersion;

        [JsonIgnore]
        public string SolutionFileNameActual => string.IsNullOrEmpty(SolutionFileName) ? _lazySolutionFileName.Value : SolutionFileName;

        [JsonIgnore]
        public static string FileName
        {
            get
            {
                var currentDirectory = Directory.GetCurrentDirectory();
                var fileName = Path.Combine(currentDirectory, DefaultFileName);
                return fileName;
            }
        }

        public static Config Load()
        {
            var json = File.ReadAllText(FileName);
            var config = JsonConvert.DeserializeObject<Config>(json);
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
    }
}