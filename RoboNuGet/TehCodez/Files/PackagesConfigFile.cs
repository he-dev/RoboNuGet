using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using JetBrains.Annotations;

namespace RoboNuGet.Files
{
    internal class PackagesConfigFile
    {
        private const string DefaultFileName = "packages.config";

        private PackagesConfigFile(string fileName, IEnumerable<PackageElement> packages)
        {
            FileName = fileName;
            Packages = packages;
        }

        [NotNull]
        private string FileName { get; }

        [NotNull, ItemNotNull]
        public IEnumerable<PackageElement> Packages { get; }

        [NotNull]
        public static PackagesConfigFile Load(string directoryName)
        {
            var fileName = Path.Combine(directoryName, DefaultFileName);
            if (!File.Exists(fileName))
            {
                return new PackagesConfigFile(fileName, Enumerable.Empty<PackageElement>());
            }

            var packagesConfig = XDocument.Load(fileName);
            var packages =
                ((IEnumerable) packagesConfig.XPathEvaluate(@"packages/package"))
                .Cast<XElement>()
                .Select(x => new PackageElement
                {
                    Id = x.Attribute("id").Value,
                    Version = x.Attribute("version").Value
                }).ToList();

            return new PackagesConfigFile(fileName, packages);
        }
    }

    internal class PackageElement
    {
        public string Id { get; set; }
        public string Version { get; set; }
    }
}