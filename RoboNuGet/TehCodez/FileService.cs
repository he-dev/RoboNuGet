using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Exceptionize;
using RoboNuGet.Files;

namespace RoboNuGet
{
    internal interface IFileService
    {
        [NotNull]
        string GetSolutionFileName([NotNull] string path);

        [NotNull, ItemNotNull]
        IEnumerable<NuspecFile> GetNuspecFiles([NotNull] string solutionDirectoryName);
    }
    
    [UsedImplicitly]
    internal class FileService : IFileService
    {
        public string GetSolutionFileName(string pathOrSearchPattern)
        {
            if (Path.IsPathRooted(pathOrSearchPattern))
            {
                return pathOrSearchPattern;
            }

            var directory = Directory.GetParent(Directory.GetCurrentDirectory());
            do
            {
                var solutionFiles = Directory.GetFiles(directory.FullName, pathOrSearchPattern);

                if (solutionFiles.Length > 1)
                {
                    throw DynamicException.Factory.CreateDynamicException($"MultipleSolutionFiles{nameof(Exception)}", "Filter matches multiple solution files.", null);
                }

                if (solutionFiles.Length == 1)
                {
                    return solutionFiles.First();
                }

                directory = Directory.GetParent(directory.FullName);
            } while (directory.Parent != null);

            throw DynamicException.Factory.CreateDynamicException($"SolutionFileNotFound{nameof(Exception)}", "Solution file not found.", null);
        }

        public IEnumerable<NuspecFile> GetNuspecFiles(string solutionDirectoryName)
        {
            var directories = Directory.GetDirectories(solutionDirectoryName);
            foreach (var directory in directories)
            {
                var packageNuspec = NuspecFile.From(directory);
                if (packageNuspec == null)
                {
                    continue;
                }
                yield return packageNuspec;
            }
        }
    }
}