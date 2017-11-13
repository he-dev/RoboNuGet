using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reusable;
using Reusable.ConsoleColorizer;
using Reusable.Extensions;
using Reusable.MarkupBuilder.Html;
using Reusable.OmniLog;

namespace RoboNuGet
{
    internal static class LoggerHelper
    {
        public static ILogger ConsoleError(this ILogger logger, string message)
        {
            return logger.ConsoleMessageLine(m => m
                .Indent()
                .span(s => s
                    .text(message)
                    .color(ConsoleColor.Red)));
        }

        public static ILogger ConsoleException(this ILogger logger, Exception exception)
        {
            return logger.ConsoleMessageLine(m => m
                .Indent()
                .span(s => s
                    .text($"{exception.GetType().Name}: {exception.Message}")
                    .color(ConsoleColor.Red)));
        }


        public static HtmlElement Prompt(this HtmlElement consoleTemplate)
        {
            return consoleTemplate.span(s => s
                .text($"{DateTime.UtcNow.ToString(CultureInfo.InvariantCulture).EncloseWith("[]")}>")
                .color(ConsoleColor.DarkGray));
        }

        public static HtmlElement Indent(this HtmlElement consoleTemplate, int depth = 1)
        {
            return consoleTemplate.span(s => s.text(new string(' ', depth)));
        }
    }
}