using System;
using System.IO;
using System.Windows.Input;
using RoboNuGet.Data;

namespace RoboNuGet.Commands
{
    internal class IncrementPathVersionCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return ((dynamic)parameter).AutoIncrementPatchVersion;
        }

        public void Execute(object parameter)
        {
            var config = (Config)parameter;
            config.IncrementPatchVersion();
            config.Save();
        }

    }
}