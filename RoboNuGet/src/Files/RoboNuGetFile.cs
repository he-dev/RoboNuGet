using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable;
using Reusable.Exceptionize;

namespace RoboNuGet.Files
{
    [UsedImplicitly]
    internal class RoboNuGetFile
    {
        private static readonly JsonSerializerSettings DefaultSerializerSettings = new JsonSerializerSettings
        {
            //Converters = {new JsonOverridableConverter()},
            Formatting = Formatting.Indented,
            DefaultValueHandling = DefaultValueHandling.Populate
        };

        private const string DefaultFileName = "RoboNuGet.json";

        public IEnumerable<string> ExcludeDirectories { get; set; } = Enumerable.Empty<string>(); 

        [JsonRequired]
        public IEnumerable<Solution> Solutions { get; set; } = default!;

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

        public static RoboNuGetFile Load()
        {
            var json = File.ReadAllText(FileName);
            return JsonConvert.DeserializeObject<RoboNuGetFile>(json, DefaultSerializerSettings) ?? throw DynamicException.Create("Configuration", $"Could not load '{FileName}'.");
        }

        public void Save()
        {
            var json = JsonConvert.SerializeObject(this, DefaultSerializerSettings);
            File.WriteAllText(FileName, json);
        }
    }

    internal class Solution
    {
        //[DefaultValue("*.sln")]
        [JsonRequired]
        public string FileName { get; set; } = default!;

        [JsonIgnore]
        public string DirectoryName => Path.GetDirectoryName(FileName);

        [JsonRequired]
        public string PackageVersion { get; set; } = default!;

        public bool IsPrerelease { get; set; }

        [JsonIgnore]
        public string FullVersion => $"{PackageVersion}{(IsPrerelease ? "-pre" : string.Empty)}";

        [JsonRequired]
        public MsBuild MsBuild { get; set; } = default!;

        [JsonRequired]
        public NuGet NuGet { get; set; } = default!;
    }

    [UsedImplicitly]
    internal class NuGet
    {
        [JsonRequired]
        public string OutputDirectoryName { get; set; } = default!;

        [JsonRequired]
        public string NuGetConfigName { get; set; } = default!;

        [JsonRequired]
        public Dictionary<SoftString, string> Commands { get; set; } = default!;
    }
}