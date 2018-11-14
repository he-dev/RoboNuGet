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
        private readonly string _excludeDirectoriesPattern;

        public SolutionDirectoryTree(IDirectoryTree directoryTree, RoboNuGetFile roboNuGetFile)
        {
            _directoryTree = directoryTree;
            _excludeDirectoriesPattern = roboNuGetFile.ExcludeDirectories.Select(Regex.Escape).Join("|");
        }        
        
        [NotNull, ItemNotNull]
        public IEnumerable<NuspecFile> FindNuspecFiles(string solutionDirectoryName)
        {            
            return
                _directoryTree
                    .Walk(solutionDirectoryName, DirectoryTree.MaxDepth(2), DirectoryTree.IgnoreExceptions)
                    .SkipDirectories($"({_excludeDirectoriesPattern})")
                    .WhereFiles("\\.nuspec$")
                    .SelectMany(node => node.FileNames.Select(name => Path.Combine(node.DirectoryName, name)))
                    .Select(NuspecFile.Load);
        }
    }
}