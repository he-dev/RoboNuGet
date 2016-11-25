using System;
using RoboNuGet.Data;

namespace RoboNuGet.Commands
{
    internal class BuildCommand : StartProcessCommand
    {
        //public MsBuild MsBuild { get; set; }

        public override void Execute(dynamic parameter)
        {
            parameter.MsBuild.ProjectFile = parameter.SolutionFile;

            base.Execute(new
            {
                FileName = "msbuild",
                Arguments = parameter.MsBuild.ToString()
            });
        }

    }
}