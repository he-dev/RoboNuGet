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
    internal class SolutionDirectoryTree
    {
        private readonly IDirectoryTree _directoryTree;
        private readonly Session _session;

        public SolutionDirectoryTree(IDirectoryTree directoryTree, Session session)
        {
            _directoryTree = directoryTree;
            _session = session;
        }

        [NotNull, ItemNotNull]
        public IEnumerable<NuspecFile> FindNuspecFiles(string solutionDirectoryName)
        {
            return
                _directoryTree
                    .Walk(solutionDirectoryName, PhysicalDirectoryTree.MaxDepth(2), PhysicalDirectoryTree.IgnoreExceptions)
                    .SkipDirectories($"({CreateDirectoryFilter()})")
                    .WhereFiles("\\.nuspec$")
                    .SelectMany(node => node.FileNames.Select(name => Path.Combine(node.DirectoryName, name)))
                    .Select(NuspecFile.Load);
        }

        // This needs to be used during package updates because otherwise read/write occurs at the same time due to async.
        public NuspecFile GetNuspecFile(string solutionDirectoryName, string packageId)
        {
            return
                _directoryTree
                    .Walk(solutionDirectoryName, PhysicalDirectoryTree.MaxDepth(2), PhysicalDirectoryTree.IgnoreExceptions)
                    .SkipDirectories($"({CreateDirectoryFilter()})")
                    .WhereFiles($"{Regex.Escape(packageId)}\\.nuspec$")
                    .SelectMany(node => node.FileNames.Select(name => Path.Combine(node.DirectoryName, name)))
                    .Select(NuspecFile.Load)
                    .SingleOrThrow(onEmpty:("NuspecFileNotFound", $"Could not find nuspec-file '{packageId}'"));
        }

        private string CreateDirectoryFilter() => _session.Config.ExcludeDirectories.Select(Regex.Escape).Join("|");
    }
}