using System.Xml.Linq;

namespace RoboNuGet.Files
{
    internal class NuspecDependency
    {
        public NuspecDependency(string id, string version)
        {
            Id = id;
            Version = version;
        }

        public string Id { get; }

        public string Version { get; }

        public XElement ToXElement()
        {
            return new XElement(
                "dependency",
                new XAttribute("id", Id),
                new XAttribute("version", Version)
            );
        }
    }
}