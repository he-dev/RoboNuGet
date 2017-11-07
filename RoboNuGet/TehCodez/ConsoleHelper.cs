using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reusable;
using Reusable.ConsoleColorizer;
using Reusable.Extensions;
using Reusable.OmniLog;

namespace RoboNuGet
{
    internal static class ConsoleHelper
    {
        public static ILogger ConsoleError(this ILogger logger, string message)
        {
           return logger.ConsoleParagraph(p => p.Indent().ConsoleSpan(ConsoleColor.Red, null, _ => message));
        }
        
        public static ILogger ConsoleException(this ILogger logger, Exception exception)
        {
            return logger.ConsoleParagraph(p => p.Indent().ConsoleSpan(ConsoleColor.Red, null, _ => $"{exception.GetType().Name}: {exception.Message}"));
        }
            
            
        public static ConsoleTemplate Prompt(this ConsoleTemplate consoleTemplate)
        {
            return consoleTemplate.ConsoleSpan(ConsoleColor.DarkGray, null, s => s.ConsoleText($"{DateTime.UtcNow.ToString(CultureInfo.InvariantCulture).EncloseWith("[]")}>"));
        }
        
        public static ConsoleTemplate Indent(this ConsoleTemplate consoleTemplate, int depth = 1)
        {
            return consoleTemplate.ConsoleSpan(null, null, s => s.ConsoleText(new string(' ', depth)));
        }
    }
}
