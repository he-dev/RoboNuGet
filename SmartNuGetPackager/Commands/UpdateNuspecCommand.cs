﻿using System;
using System.IO;
using System.Windows.Input;
using RoboNuGet.Data;

namespace RoboNuGet.Commands
{
    internal class UpdateNuspecCommand : ICommand
    {
        public string PackagesDirectoryName { get; set; }

        public string PackageId { get; set; }

        public string Version { get; set; }

        public string NuGetConfigFileName { get; set; }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(dynamic parameter)
        {
            dynamic packageNuspec = parameter.PackageNuspec;
            dynamic packageVersion = parameter.PackageVersion;

            var directory = Path.GetDirectoryName(packageNuspec.FileName);
            var packagesConfig = PackagesConfig.From(directory);
            var csProj = CsProj.From(directory);

            foreach (var package in packagesConfig.Packages)
            {
                packageNuspec.AddDependency(package.Id, package.Version);
            }

            foreach (var projectReferenceName in csProj.ProjectReferenceNames)
            {
                packageNuspec.AddDependency(projectReferenceName, packageVersion);
            }

            packageNuspec.Version = packageVersion;
            packageNuspec.Save();
        }        
    }
}