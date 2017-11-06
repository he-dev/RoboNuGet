using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Exceptionize;
using Reusable.Extensions;
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
            const int notFound = 0;
            const int singleFile = 1;

            // Searching for *.nuspec only one level deep.
            foreach (var directory in Directory.GetDirectories(solutionDirectoryName))
            {
                var files = Directory.GetFiles(directory, $"*{NuspecFile.DefaultExtension}");
                switch (files.Length)
                {
                    case notFound:
                        break;

                    case singleFile:
                        yield return NuspecFile.Load(files.Single());
                        break;

                    default:
                        throw DynamicException.Factory.CreateDynamicException($"MultipleNuspecFiles{nameof(Exception)}", $"There can be only a single *.nuspec file. Directory: {directory.QuoteWith("'")}", null);
                }
            }
        }
    }
}