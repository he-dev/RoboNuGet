using System.Xml.Linq;

namespace RoboNuGet.Files
{
    internal class NuspecDependency
    {
        public string Id { get; set; }

        public string Version { get; set; }

        public static NuspecDependency Create(XElement xDependency)
        {
            return new NuspecDependency
            {
                Id = xDependency.Attribute("id").Value,
                Version = xDependency.Attribute("version").Value
            };
        }

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