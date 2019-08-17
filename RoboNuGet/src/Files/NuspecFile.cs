using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using System.Xml.XPath;
using JetBrains.Annotations;

namespace RoboNuGet.Files
{
    internal class NuspecFile
    {
        public const string Extension = ".nuspec";

        private readonly XDocument _xNuspec;

        private NuspecFile(string fileName, XDocument xNuspec)
        {
            FileName = fileName;
            _xNuspec = xNuspec;
        }

        public string FileName { get; }

        [XPath(@"package/metadata/id")]
        public string Id => XPathSelectElements().Single().Value;

        [XPath(@"package/metadata/version")]
        public string Version
        {
            get => XPathSelectElements().Single().Value;
            set => XPathSelectElements().Single().Value = value;
        }

        [XPath(@"package/metadata/dependencies")]
        public IEnumerable<NuspecDependency> Dependencies
        {
            get
            {
                return
                    from xDependency in XPathSelectElements().SingleOrDefault()?.Elements() ?? Enumerable.Empty<XElement>()
                    select NuspecDependency.Create(xDependency);
            }
            set
            {
                var xDependencies = XPathSelectElements().SingleOrDefault();

                if (xDependencies is null)
                {
                    var xMetadata = _xNuspec.XPathSelectElements(@"package/metadata").Single();
                    xMetadata.Add(xDependencies = new XElement("dependencies"));
                }

                if (value is null)
                {
                    xDependencies.Remove();
                }
                else
                {
                    xDependencies.RemoveAll();
                    foreach (var dependency in value)
                    {
                        xDependencies.Add(dependency.ToXElement());
                    }
                }
            }
        }

        [NotNull]
        public static NuspecFile Load(string fileName)
        {
            //using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                //var xNuspec = XDocument.Load(fs.Rewind());
                var xNuspec = XDocument.Load(fileName);
                return new NuspecFile(fileName, xNuspec);
            }
        }

        public void Save()
        {
            _xNuspec.Save(FileName, SaveOptions.None);
        }

        private IEnumerable<XElement> XPathSelectElements([CallerMemberName] string memeberName = null)
        {
            var xPath = typeof(NuspecFile).GetProperty(memeberName).GetCustomAttribute<XPathAttribute>();
            return _xNuspec.XPathSelectElements(xPath);
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    internal class XPathAttribute : Attribute
    {
        private readonly string _xPath;

        public XPathAttribute(string xPath) => _xPath = xPath;

        public override string ToString() => _xPath;

        public static implicit operator string(XPathAttribute xPathAttribute) => xPathAttribute.ToString();
    }

    public static class XDocumentExtensions
    {
        public static IEnumerable<XElement> XPathSelectElements<T>(this XDocument xDocument, [CallerMemberName] string memeberName = null)
        {
            var xPath = typeof(T).GetProperty(memeberName).GetCustomAttribute<XPathAttribute>();
            return xDocument.XPathSelectElements(xPath);
        }
    }
}