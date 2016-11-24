using System;
using System.IO;
using RoboNuGet.Data;

namespace RoboNuGet.Commands
{
    internal class PackCommand : StartProcessCommand
    {
        public PackCommand()
        {
            RedirectStandardOutput = true;
        }

        public const string Name = "pack";

        public PackageNuspec PackageNuspec { get; set; }

        public string Version { get; set; }

        public string Outputdirectory { get; set; }

        public override void Execute(object parameter)
        {
            base.Execute(new
            {
                FileName = "nuget",
                Arguments = CreatePackCommand()
            });
        }

        private string CreatePackCommand()
        {
            // todo: move to config
            return
                $"pack " +
                $"\"{PackageNuspec.FileName}\" " +
                $"-properties Configuration=Release " +
                $"-outputdirectory {Outputdirectory}";
        }
    }
}