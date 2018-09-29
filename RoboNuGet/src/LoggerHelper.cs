using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reusable;
using Reusable.Extensions;
using Reusable.MarkupBuilder.Html;
using Reusable.OmniLog;

namespace RoboNuGet
{
    internal static class LoggerHelper
    {
        public static ILogger ConsoleError(this ILogger logger, string message)
        {
            return logger.WriteLine(m => m
                .Indent()
                .span(s => s
                    .text(message)
                    .color(ConsoleColor.Red)));
        }

        public static ILogger ConsoleException(this ILogger logger, Exception exception)
        {
            foreach (var (ex, _) in exception.SelectMany())
            {
                logger
                    .WriteLine(m => m
                        .Indent()
                        .span(s => s
                            .text($"{ex.GetType().Name}: {ex.Message}")
                            .color(ConsoleColor.Red)));
            }

            return logger;
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