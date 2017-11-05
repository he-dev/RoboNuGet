using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using JetBrains.Annotations;
using Reusable.Commander;
using Reusable.OmniLog;

namespace RoboNuGet.Commands
{
    internal abstract class StartProcess : ConsoleCommand
    {
        protected StartProcess(ILoggerFactory loggerFactory) : base(loggerFactory)
        {
        }
        
        protected bool RedirectStandardOutput { get; set; }

        protected string FileName { get; set; }

        protected string Arguments { get; set; }

        public override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "cmd",
                Arguments =
                    RedirectStandardOutput
                        ? $"/Q /C {FileName} {Arguments}"
                        : $"/Q /C pause | {FileName} {Arguments}",

                RedirectStandardOutput = RedirectStandardOutput,
                UseShellExecute = !RedirectStandardOutput
            };

            // todo this needs a better exception
            var process = Process.Start(processStartInfo) ?? throw new Exception("Could not start process.");
            
            if (RedirectStandardOutput)
            {
                Console.WriteLine(process.StandardOutput.ReadToEnd());
            }
            process.WaitForExit();
            if (process.ExitCode != 0)
            {
                throw new Exception($"Process did not exit smoothly: {process.ExitCode}");
            }

            return Task.CompletedTask;
        }
    }
}