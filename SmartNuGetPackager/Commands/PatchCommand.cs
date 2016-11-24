using System;
using System.IO;
using System.Windows.Input;
using RoboNuGet.Data;

namespace RoboNuGet.Commands
{
    internal class PatchCommand : ICommand, IIdentifiable
    {
        public string Name => "patch";

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            var config = (Config)parameter;
            config.IncrementPatchVersion();
            config.Save();
        }

    }
}