using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Custom;
using JetBrains.Annotations;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.IO;
using RoboNuGet.Files;

namespace RoboNuGet
{
    internal interface IFileSearch
    {
        [NotNull]
        string FindSolutionFile();

        [NotNull, ItemNotNull]
        IEnumerable<NuspecFile> FindNuspecFiles();
    }

    [UsedImplicitly]
    internal class FileSearch : IFileSearch
    {
        private readonly IFileSystem _fileSystem;
        private readonly RoboNuGetFile _roboNuGetFile;

        public FileSearch(IFileSystem fileSystem, RoboNuGetFile roboNuGetFile)
        {
            _fileSystem = fileSystem;
            _roboNuGetFile = roboNuGetFile;
        }

        public string FindSolutionFile()
        {
            var solutionFileFilter = FileFilterFactory.Default.Create(_roboNuGetFile.SolutionFileName);
            var files =
                _fileSystem
                    .EnumerateDirectories(_roboNuGetFile.SolutionDirectoryName, GetDirectoryFilter())
                    .SelectMany(_fileSystem.EnumerateFiles)
                    .Where(solutionFileFilter)
                    .ToList();

            const int notFound = 0;
            const int exactMatch = 1;

            switch (files.Count)
            {
                case notFound:
                    throw DynamicException.Factory.CreateDynamicException(
                        $"SolutionFileNotFound{nameof(Exception)}",
                        $"Solution file {_roboNuGetFile.SolutionFileName.QuoteWith("'")} not found in {_roboNuGetFile.SolutionDirectoryName.QuoteWith("'")}.", null);

                case exactMatch:
                    return files.Single();

                default:
                    throw DynamicException.Factory.CreateDynamicException(
                        $"MultipleSolutionFiles{nameof(Exception)}",
                        $"Multiple solution files {_roboNuGetFile.SolutionFileName.QuoteWith("'")} found in {_roboNuGetFile.SolutionDirectoryName.QuoteWith("'")}.", null);
            }
        }

        public IEnumerable<NuspecFile> FindNuspecFiles()
        {
            var solutionFileFilter = FileFilterFactory.Default.Create(NuspecFile.DefaultExtension);
            var nuspecFileNames =
                _fileSystem
                    .EnumerateDirectories(_roboNuGetFile.SolutionDirectoryName, GetDirectoryFilter())
                    .SelectMany(_fileSystem.EnumerateFiles)
                    .Where(solutionFileFilter);

            // Searching for *.nuspec only one level deep.
            foreach (var nuspecFileName in nuspecFileNames)
            {
                yield return NuspecFile.Load(nuspecFileName);
            }
        }

        private Func<string, bool> GetDirectoryFilter()
        {
            return 
                _roboNuGetFile
                    .ExcludeDirectories
                    .Select(DirectoryFilterFactory.Default.Create)
                    .ToAny();
        }
    }
}