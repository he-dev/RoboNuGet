using System;
using System.IO;
using System.Windows.Input;

namespace RoboNuGet.Commands
{
    internal class VersionCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(dynamic parameter)
        {
            // todo: needs validation with semantic version
            
            parameter.Config.PackageVersion = parameter.Version;
            parameter.Config.Save();

        }
    }
}