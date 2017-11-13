using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using JetBrains.Annotations;
using Reusable.Commander;
using Reusable.ConsoleColorizer;
using Reusable.Exceptionize;
using Reusable.MarkupBuilder.Html;
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
        
        public Progress<string> Progress { get; } = new Progress<string>();

        public string Output { get; private set; }

        public int ExitCode { get; private set; }

        public override async Task ExecuteAsync(CancellationToken cancellationToken)
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

            var process = Process.Start(processStartInfo) ?? throw DynamicException.Factory.CreateDynamicException($"ProcessStart{nameof(Exception)}", $"Could not start process: {Arguments}", null);
            
            if (RedirectStandardOutput)
            {
                //((IProgress<string>)Progress).Report(process.StandardOutput.ReadToEnd());

                Output = await process.StandardOutput.ReadToEndAsync();
                

                //Logger.ConsoleMessageLine(m => m.text(process.StandardOutput.ReadToEnd()));
            }
            
            process.WaitForExit();
            if (process.ExitCode != 0)
            {
                throw DynamicException.Factory.CreateDynamicException($"ProcessExit{nameof(Exception)}", $"Process did not exit smoothly. Exit code: {process.ExitCode}. CommandLine: {Arguments}", null);
            }
        }
    }
}