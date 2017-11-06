using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reusable;
using Reusable.ConsoleColorizer;
using Reusable.Extensions;

namespace RoboNuGet
{
    internal static class Picasso
    {
        public static void WriteError(string message)
        {
            //ConsoleColorizer.Render($"<p>&gt;<span fg=\"red\">ERROR:</span> {message} <span fg=\"darkyellow\">(Press Enter to exit)</span></p>");
            Console.ReadKey();
        }
    }

    internal static class ConsoleTemplateExtensions
    {
        public static ConsoleTemplate Prompt(this ConsoleTemplate consoleTemplate)
        {
            return consoleTemplate.ConsoleSpan(ConsoleColor.DarkGray, null, s => s.ConsoleText($"{DateTime.UtcNow.ToString(CultureInfo.InvariantCulture).EncloseWith("[]")}>"));
        }
    }
}
