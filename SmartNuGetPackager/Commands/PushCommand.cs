using System;
using System.IO;

namespace RoboNuGet.Commands
{
    internal class PushCommand : StartProcessCommand
    {
        public const string Name = "push";

        public PushCommand()
        {
            RedirectStandardOutput = true;
        }

        public string PackagesDirectoryName { get; set; }

        public string PackageId { get; set; }

        public string Version { get; set; }

        public string NuGetConfigFileName { get; set; }

        public void Execute(object parameter)
        {
            throw new NotImplementedException();
        }

        protected override bool ExecuteCore()
        {
            return Execute("nuget", CreatePushCommand());
        }

        private string CreatePushCommand()
        {
            // todo: move to config
            var nupkgFileName = $"{Path.Combine(PackagesDirectoryName, $"{PackageId}.{Version}.nupkg")}";
            return
                $"push " +
                $"\"{nupkgFileName}\" " +
                $"-configfile {NuGetConfigFileName}";
        }
    }
}