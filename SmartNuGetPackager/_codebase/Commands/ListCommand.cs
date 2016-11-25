using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Input;
using RoboNuGet.Data;

namespace RoboNuGet.Commands
{
    internal class ListCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(dynamic parameter)
        {
            var packageNuspecs = (IEnumerable<PackageNuspec>)parameter.PackageNuspecs;

            foreach (var packageNuspec in packageNuspecs)
            {
                Console.WriteLine();
                Console.WriteLine($"{Path.GetFileNameWithoutExtension(packageNuspec.FileName)} ({packageNuspec.Dependencies.Count()})");

                foreach (var nuspecDependency in packageNuspec.Dependencies)
                {
                    Console.WriteLine($"- {nuspecDependency.Id} v{nuspecDependency.Version}");
                }
            }

            Console.ReadKey();
        }
    }
}