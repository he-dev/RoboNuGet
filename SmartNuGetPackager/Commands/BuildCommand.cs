using System;
using RoboNuGet.Data;

namespace RoboNuGet.Commands
{
    internal class BuildCommand : StartProcessCommand, IIdentifiable
    {
        public string Name => "build";

        public MsBuild MsBuild { get; set; }

        public override void Execute(object parameter)
        {
            base.Execute(new
            {
                FileName = "msbuild",
                Arguments = MsBuild.ToString()
            });
        }

    }
}