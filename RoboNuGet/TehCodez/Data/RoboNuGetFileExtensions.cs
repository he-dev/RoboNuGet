using System.Collections.Generic;
using System.IO;
using RoboNuGet.Files;

namespace RoboNuGet.Data
{
    internal static class RoboNuGetFileExtensions
    {
        public static string GetSolutionFileName(this RoboNuGetFile roboNuGetFile, IFileService fileService)
        {
            return fileService.GetSolutionFileName(roboNuGetFile.SolutionFileName);
        }

        public static IEnumerable<NuspecFile> GetNuspecFiles(this RoboNuGetFile roboNuGetFile, IFileService fileService)
        {
            var solutionFileName = roboNuGetFile.GetSolutionFileName(fileService);
            var solutionDirectoryName =  Path.GetDirectoryName(solutionFileName);
            
            // ReSharper disable once AssignNullToNotNullAttribute - there's not chance it's null
            return fileService.GetNuspecFiles(solutionDirectoryName);
        }
    }
}