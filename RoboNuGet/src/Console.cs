using System;
using System.Globalization;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.MarkupBuilder;
using Reusable.MarkupBuilder.Html;

// ReSharper disable once CheckNamespace
namespace RoboNuGet.Console
{
    using static ConsoleColor;
    using static Templates;

    namespace Models
    {
        public abstract class Model : Reusable.OmniLog.Console.Model
        {
            public DateTime Timestamp => DateTime.Now;
        }

        public class ProgramInfo : Model
        {
            public string Name => RoboNuGet.ProgramInfo.Name;

            public string Version => RoboNuGet.ProgramInfo.Version;

            public override HtmlElement Template { get; } =
                HtmlElement
                    .Builder
                    .span(x => x
                        .Prompt()
                        .span(s => s.text("{name} v{version}").color(DarkGray)))
                    .NewLine();
        }

        public class SolutionInfo : Model
        {
            public string Name { get; set; }

            public string Version { get; set; }

            public int NuspecFileCount { get; set; }

            public override HtmlElement Template =>
                HtmlElement
                    .Builder
                    .span(span => span
                        .Prompt()
                        .text("Solution")
                        .span(s => s.text($" '{Name}'").color(Yellow))
                        .text(" ")
                        .span(s => s.text($"v{Version}").color(Magenta))
                        .text(" ")
                        .text($" ({NuspecFileCount} package{(NuspecFileCount == 1 ? string.Empty : "s")})"))
                    .NewLine();
        }

        public class SelectSolution : Model
        {
            public override HtmlElement Template =>
                HtmlElement
                    .Builder
                    .span(span => span.text("Use the 'select' command to pick a solution."))
                    .NewLine();
        }

        public class PackageInfo : Model
        {
            public string PackageId { get; set; }

            public int DependencyCount { get; set; }

            public override HtmlElement Template =>
                HtmlElement
                    .Builder
                    .span(x => x
                        .Indent(width: 1)
                        .text($"{PackageId} ")
                        .span(s => s.text($"({DependencyCount})").color(Magenta)))
                    .NewLine();
        }

        public class PackageDependencySection : Model
        {
            public string Name { get; set; }

            public override HtmlElement Template =>
                HtmlElement
                    .Builder
                    .span(x => x
                        .Indent(width: 1, depth: 2)
                        .span(s => s.text($"[{Name}]").color(DarkGray)))
                    .NewLine();
        }

        public class PackageDependencyInfo : Model
        {
            public string Name { get; set; }

            public string Version { get; set; }

            public override HtmlElement Template =>
                HtmlElement
                    .Builder
                    .span(x => x
                        .Indent(width: 1, depth: 3)
                        .text($"{Name} ")
                        .span(s => s.text($"v{Version}").color(DarkGray)))
                    .NewLine();
        }

        public class NuGetPackResultError : Model
        {
            public int ErrorCount { get; set; }

            public override HtmlElement Template =>
                HtmlElement
                    .Builder
                    .span(x => x
                        .Indent(width: 1)
                        .span(s => s
                            .text($"{ErrorCount} error(s) occured.")
                            .color(Red)))
                    .NewLine();
        }

        public class NuGetPackResultSuccess : Model
        {
            public override HtmlElement Template =>
                HtmlElement
                    .Builder
                    .span(x => x
                        .Indent(width: 1)
                        .span(s => s.text("All packages successfully created.").color(Green)))
                    .NewLine();
        }

        public class NuGetCommandInfo : Model
        {
            public TimeSpan Elapsed { get; set; }

            public int ThreadId { get; set; }

            public override HtmlElement Template =>
                HtmlElement
                    .Builder
                    .span(x => x
                        .Indent(width: 1)
                        .text($"Elapsed: {Elapsed.TotalSeconds:F1} sec [{ThreadId}]"))
                    .NewLine();
        }
    }

    public static class Templates
    {
        public static HtmlElement Prompt(this HtmlElement element)
        {
            return element.span(span => span.text("[{Timestamp:yyyy-MM-dd HH:mm:ss}]>"));
        }

        public static HtmlElement NewLine(this HtmlElement element, ConsoleColor color = DarkGray)
        {
            return HtmlElement.Builder.p(p => p.Append(element).color(color));
        }

        public static HtmlElement Indent(this HtmlElement element, int width = 3, int depth = 1)
        {
            return element.span(s => s.text(new string(' ', width * depth)));
        }
    }
}