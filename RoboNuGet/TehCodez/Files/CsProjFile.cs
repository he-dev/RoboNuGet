using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;
using JetBrains.Annotations;
using Reusable;
using Reusable.Exceptionize;
using Reusable.Extensions;

namespace RoboNuGet.Files
{
    internal class CsProjFile
    {
        public const string DefaultExtension = ".csproj";

        private CsProjFile([NotNull] IEnumerable<string> projectReferences, IEnumerable<PackageElement> packageReferences, bool isNewFormat)
        {
            IsNewFormat = isNewFormat;
            ProjectReferences = projectReferences?.ToList() ?? throw new ArgumentNullException(nameof(projectReferences));
            PackageReferences = packageReferences;
        }

        public bool IsNewFormat { get; }

        public IEnumerable<string> ProjectReferences { get; }

        public IEnumerable<PackageElement> PackageReferences { get; }

        [NotNull]
        public static CsProjFile Load([NotNull] string fileName)
        {
            if (fileName == null) throw new ArgumentNullException(nameof(fileName));

            if (!File.Exists(fileName))
            {
                throw DynamicException.Factory.CreateDynamicException(
                    $"CsProjFileNotFound{nameof(Exception)}",
                    $"File {fileName.QuoteWith("'")} does not exist.", null);
            }

            var csproj = XDocument.Load(fileName);

            var isNewFormat = csproj.Root.Descendants().Any(IsNewFormat());

            var projectReferenceNames =
                csproj
                    .XPathSelectElements("//*[contains(local-name(), 'ProjectReference')]")
                    .Select(projectReference => projectReference.Attribute("Include").Value)
                    .Select(Path.GetFileNameWithoutExtension);
            //.Select(x => x.Element(XName.Get("Name", csproj.Root.GetDefaultNamespace().NamespaceName)).Value);

            var packageReferences =
                csproj
                    .XPathSelectElements("//*[contains(local-name(), 'PackageReference')]")
                    .Select(projectReference => new PackageElement
                    {
                        Id = projectReference.Attribute("Include").Value,
                        Version = projectReference.Attribute("Version").Value
                    });

            return new CsProjFile(projectReferenceNames, packageReferences, isNewFormat);

            Func<XElement, bool> IsNewFormat()
            {
                return element => Regex.IsMatch(element.Name.LocalName, @"\ATargetFramework(s)?\Z", RegexOptions.IgnoreCase);
            }
        }
    }
}