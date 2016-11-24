using System;
using System.Diagnostics;
using System.Windows.Input;

namespace RoboNuGet.Commands
{
    internal abstract class StartProcessCommand : ICommand
    {
        protected bool RedirectStandardOutput { get; set; }

        public event EventHandler CanExecuteChanged;

        public virtual bool CanExecute(object parameter)
        {
            return true;
        }

        public virtual void Execute(dynamic parameter)
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "cmd",
                Arguments = 
                    RedirectStandardOutput 
                    ? $"/Q /C {parameter.FileName} {parameter.Arguments}" 
                    : $"/Q /C pause | {parameter.FileName} {parameter.Arguments}",

                RedirectStandardOutput = RedirectStandardOutput,
                UseShellExecute = !RedirectStandardOutput
            };

            var process = Process.Start(processStartInfo);
            if (RedirectStandardOutput)
            {
                Console.WriteLine(process.StandardOutput.ReadToEnd());
            }
            process.WaitForExit();
            if (process.ExitCode != 0)
            {
                throw new Exception($"Process did not exit smoothly: {process.ExitCode}");
            }
        }
    }
}