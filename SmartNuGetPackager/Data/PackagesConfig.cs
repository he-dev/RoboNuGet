using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;

namespace RoboNuGet.Data
{
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
}