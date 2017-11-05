using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using RoboNuGet.Data;

namespace RoboNuGet.Files
{
    internal class NuspecFile
    {
        private readonly XDocument _packageNuspec;

        public NuspecFile(string fileName)
        {
            _packageNuspec = XDocument.Load(FileName = fileName);
        }

        public static NuspecFile From(string dirName)
        {
            var packageNuspecFileName = Directory.GetFiles(dirName, "*.nuspec").SingleOrDefault();
            return string.IsNullOrEmpty(packageNuspecFileName) ? null : new NuspecFile(packageNuspecFileName);
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

        public IEnumerable<PackageNuspecDependency> Dependencies
        {
            get
            {
                var xDependencies = ((IEnumerable)_packageNuspec.XPathEvaluate(@"package/metadata/dependencies")).Cast<XElement>().SingleOrDefault();
                return xDependencies?.Elements().Select(x => new PackageNuspecDependency
                (
                    id: x.Attribute("id").Value,
                    version: x.Attribute("version").Value)
                ) 
                ?? Enumerable.Empty<PackageNuspecDependency>();
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

        public void ClearDependencies()
        {
            var xDependencies = ((IEnumerable)_packageNuspec.XPathEvaluate(@"package/metadata/dependencies")).Cast<XElement>().SingleOrDefault();
            xDependencies?.Remove();
        }

        public void Save()
        {
            _packageNuspec.Save(FileName, SaveOptions.None);
        }
    }
}