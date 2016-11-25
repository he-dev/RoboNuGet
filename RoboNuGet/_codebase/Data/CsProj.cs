using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;

namespace RoboNuGet.Data
{
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