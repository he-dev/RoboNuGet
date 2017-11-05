using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reusable;

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
}
