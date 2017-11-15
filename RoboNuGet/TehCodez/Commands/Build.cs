using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable;
using Reusable.Commander;
using Reusable.Extensions;
using Reusable.OmniLog;
using RoboNuGet.Files;

namespace RoboNuGet.Commands
{
    [UsedImplicitly]
    internal class Build : ConsoleCommand
    {
        private readonly RoboNuGetFile _roboNuGetFile;
        private readonly IFileSearch _fileSearch;

        public Build(ILoggerFactory loggerFactory, RoboNuGetFile roboNuGetFile, IFileSearch fileSearch) : base(loggerFactory)
        {
            _roboNuGetFile = roboNuGetFile;
            _fileSearch = fileSearch;
            //FileName = "msbuild";
        }

        public override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var solutionFileName = _fileSearch.FindSolutionFile();
            var arguments = _roboNuGetFile.MsBuild.ToString(solutionFileName);
            
            var processExecutor = new ProcessExecutor();

            var result = processExecutor.ShellCmdExecute("/q /c pause |", "msbuild", arguments); //, CmdSwitch.EchoOff, CmdSwitch.Terminate);
            
            //return base.ExecuteAsync(cancellationToken);
            return Task.CompletedTask;
        }

        //public class CmdExecutor : MarshalByRefObject
        //{
        //    public CmdResult Execute([NotNull] string fileName, [NotNull] string arguments, params string[] cmdSwitches)
        //    {
        //        if (cmdSwitches == null) throw new ArgumentNullException(nameof(cmdSwitches));
        //        if (fileName == null) throw new ArgumentNullException(nameof(fileName));
        //        if (arguments == null) throw new ArgumentNullException(nameof(arguments));

        //        arguments = $"{cmdSwitches.Join(" ")} PAUSE | {fileName} {arguments}";
        //        var startInfo = new ProcessStartInfo("cmd", arguments)
        //        {
        //            UseShellExecute = true,
        //            //RedirectStandardOutput = true,
        //            //RedirectStandardError = true,
        //            //CreateNoWindow = true,
        //        };

        //        using (var process = new Process { StartInfo = startInfo })
        //        {
        //            var output = new StringBuilder();
        //            var error = new StringBuilder();

        //            //process.OutputDataReceived += (sender, e) =>
        //            //{
        //            //    output.AppendLine(e.Data);
        //            //};

        //            //process.ErrorDataReceived += (sender, e) =>
        //            //{
        //            //    error.AppendLine(e.Data);
        //            //};

        //            process.Start();
        //            //process.BeginOutputReadLine();
        //            //process.BeginErrorReadLine();
        //            process.WaitForExit();

        //            return new CmdResult
        //            {
        //                Arguments = arguments,
        //                Output = output.ToString(),
        //                Error = error.ToString(),
        //                ExitCode = process.ExitCode
        //            };
        //        }
        //    }
        //}

        //[Serializable]
        //public class CmdResult
        //{
        //    public string Arguments { get; set; }
        //    public string Output { get; set; }
        //    public string Error { get; set; }
        //    public int ExitCode { get; set; }
        //}
    }
}