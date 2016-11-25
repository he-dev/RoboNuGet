using System;
using System.IO;
using System.Windows.Input;
using RoboNuGet.Data;

namespace RoboNuGet.Commands
{
    internal class PatchCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(dynamic parameter)
        {
            var config = (Config)parameter.Config;
            config.IncrementPatchVersion();
            config.Save();
        }
    }
}