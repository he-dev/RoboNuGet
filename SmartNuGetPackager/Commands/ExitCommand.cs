using System;
using System.IO;
using System.Windows.Input;

namespace RoboNuGet.Commands
{
    internal class ExitCommand : ICommand, IIdentifiable
    {
        public string Name => "patch";

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            Environment.Exit(0);
        }

    }
}