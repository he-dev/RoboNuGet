using Reusable.Exceptionize;
using RoboNuGet.Commands;

namespace RoboNuGet.Files
{
    internal static class SessionExtensions
    {
        public static Solution SolutionOrThrow(this Session session)
        {
            return session.Solution ?? throw DynamicException.Create("SolutionNotSelected", $"You have to select a solution first. To do this, use the '{nameof(Select)}' command.");
        }
    }
}