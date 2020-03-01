using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Custom;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Reusable.IO;
using RoboNuGet.Files;

namespace RoboNuGet.Services
{
    internal class SolutionDirectory
    {
        private readonly IDirectoryTree _directoryTree;
        private readonly Session _session;

        public SolutionDirectory(IDirectoryTree directoryTree, Session session)
        {
            _directoryTree = directoryTree;
            _session = session;
        }
        
        public IEnumerable<NuspecFile> NuspecFiles(string solutionDirectoryName)
        {
            return
                _directoryTree
                    .Walk(solutionDirectoryName, DirectoryTreePredicates.MaxDepth(2), PhysicalDirectoryTree.IgnoreExceptions)
                    .IgnoreDirectories(CreateDirectoryFilter(_session.Config.ExcludeDirectories))
                    .WhereFiles("\\.nuspec$")
                    .FullNames()
                    .Select(NuspecFile.Load);
        }

        // This needs to be used during package updates because otherwise read/write occurs at the same time due to async.
        public NuspecFile GetNuspecFile(string solutionDirectoryName, string packageId)
        {
            return
                _directoryTree
                    .Walk(solutionDirectoryName, DirectoryTreePredicates.MaxDepth(2), PhysicalDirectoryTree.IgnoreExceptions)
                    .IgnoreDirectories(CreateDirectoryFilter(_session.Config.ExcludeDirectories))
                    .WhereFiles($"{Regex.Escape(packageId)}\\.nuspec$")
                    .FullNames()
                    .Select(NuspecFile.Load)
                    .SingleOrThrow(onEmpty:("NuspecFileNotFound", $"Could not find nuspec-file '{packageId}'"));
        }

        private static string CreateDirectoryFilter(IEnumerable<string> directoryNames)
        {
            return $"({directoryNames.Select(Regex.Escape).Join("|")})" ;
        }
    }
}