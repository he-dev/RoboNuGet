namespace RoboNuGet.Data
{
    internal class PackageNuspecDependency
    {
        public PackageNuspecDependency(string id, string version)
        {
            Id = id;
            Version = version;
        }

        public string Id { get; }

        public string Version { get; }
    }
}