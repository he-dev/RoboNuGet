using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace RoboNuGet.Data
{
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
}