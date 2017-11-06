using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;

namespace RoboNuGet.Files
{
    internal class CsProjFile
    {
        public const string DefaultExtension = ".csproj";
        
        private CsProjFile(IEnumerable<string> projectReferences)
        {
            ProjectReferences = projectReferences;
        }

        public IEnumerable<string> ProjectReferences { get; }

        public static CsProjFile Load(string fileName)
        {
            if (!File.Exists(fileName))
            {
                return new CsProjFile(Enumerable.Empty<string>());
            }
            
            var csproj = XDocument.Load(fileName);
            var projectReferenceNames =
                csproj.XPathSelectElements("//*[contains(local-name(), 'ProjectReference')]")
                      .Select(x => x.Element(XName.Get("Name", csproj.Root.GetDefaultNamespace().NamespaceName)).Value)
                      .ToList();

            return new CsProjFile(projectReferenceNames);
        }
    }
}