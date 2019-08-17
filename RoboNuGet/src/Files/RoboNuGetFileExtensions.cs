using Reusable.Exceptionize;
using RoboNuGet.Commands;

namespace RoboNuGet.Files
{
    internal static class RoboNuGetFileExtensions
    {
        public static SolutionInfo SelectedSolutionSafe(this RoboNuGetFile roboNuGetFile)
        {
            return roboNuGetFile.SelectedSolution ?? throw DynamicException.Create("SolutionNotSelected", $"You have to select a solution first. To do this, use the '{nameof(Select)}' command.");
        }
    }
}