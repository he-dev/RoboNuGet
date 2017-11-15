using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using JetBrains.Annotations;
using Reusable.Exceptionize;
using Reusable.Extensions;

namespace RoboNuGet.Files
{
    internal class PackagesConfigFile
    {
        private const string DefaultFileName = "packages.config";

        private readonly XDocument _xPackagesConfig;

        private PackagesConfigFile(string fileName, XDocument xPackagesConfig)
        {
            _xPackagesConfig = xPackagesConfig;
            FileName = fileName;
        }

        [NotNull]
        private string FileName { get; }

        [XPath(@"packages/package")]
        [NotNull, ItemNotNull]
        public IEnumerable<PackageElement> Packages
        {
            get
            {
                return
                    _xPackagesConfig
                        .XPathSelectElements<PackagesConfigFile>()
                        .Select(PackageElement.Create);
            }
        }

        [NotNull]
        public static PackagesConfigFile Load(string directoryName)
        {
            if (directoryName == null) throw new ArgumentNullException(nameof(directoryName));

            var fileName = Path.Combine(directoryName, DefaultFileName);

            return
                File.Exists(fileName)
                    ? new PackagesConfigFile(fileName, XDocument.Load(fileName))
                    : new PackagesConfigFile(fileName, new XDocument());
        }
    }

    internal class PackageElement
    {
        public string Id { get; set; }

        public string Version { get; set; }

        public static PackageElement Create(XElement xPackage)
        {
            return new PackageElement
            {
                Id = xPackage.Attribute("id").Value,
                Version = xPackage.Attribute("version").Value
            };
        }
    }
}