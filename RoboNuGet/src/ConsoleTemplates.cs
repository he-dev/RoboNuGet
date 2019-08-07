using System;
using System.Globalization;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.MarkupBuilder;
using Reusable.MarkupBuilder.Html;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;
using Reusable.OmniLog.Rx.ConsoleRenderers;

// ReSharper disable once CheckNamespace
namespace RoboNuGet.ConsoleTemplates
{
    using static ConsoleColor;

    public class Prompt : HtmlConsoleTemplateBuilder
    {
        public DateTime Timestamp => DateTime.Now;

        public override HtmlElement Build(LogEntry log) =>
            HtmlElement
                .Builder
                .span(span => span.text($"[{Timestamp:yyyy-MM-dd HH:mm:ss}]>"));
    }

    public class Indent : HtmlConsoleTemplateBuilder
    {
        private readonly int _depth;

        public Indent(int depth)
        {
            _depth = depth;
        }

        public int Width { get; set; } = 1;

        public override HtmlElement Build(LogEntry log) => HtmlElement.Builder.span(x => x.Indent(Width, _depth));
    }

    public class Error : HtmlConsoleTemplateBuilder
    {
        public string Text { get; set; }

        public override HtmlElement Build(LogEntry log) =>
            HtmlElement
                .Builder
                .span(x => x
                    .Indent(width: 1)
                    .text(Text)).color(Red);
    }

    public class ProgramInfo : HtmlConsoleTemplateBuilder
    {
        public string Name => RoboNuGet.ProgramInfo.Name;

        public string Version => RoboNuGet.ProgramInfo.Version;

        public override HtmlElement Build(LogEntry log) =>
            HtmlElement
                .Builder
                .span(x => x
                    .span(s => s.text($"{Name} v{Version}").color(DarkGray)));
    }

    namespace Clear
    {
        public class AskForSolution : HtmlConsoleTemplateBuilder
        {
            public override HtmlElement Build(LogEntry log) =>
                HtmlElement
                    .Builder
                    .span(span => span.text("Use the 'select <n>' command to pick a solution."));
        }

        public class SolutionOption : HtmlConsoleTemplateBuilder
        {
            public int Index { get; set; }

            public string Name { get; set; }

            public string Version { get; set; }

            public int NuspecFileCount { get; set; }

            public override HtmlElement Build(LogEntry log) =>
                HtmlElement
                    .Builder
                    .span(span => span
                        .text($"[{Index}]")
                        .span(s => s.text($" '{Name}'").color(Blue))
                        .text(" ")
                        .span(s => s.text($"v{Version}").color(Magenta))
                        .text(" ")
                        .text($" ({NuspecFileCount} package{(NuspecFileCount == 1 ? string.Empty : "s")})"));
        }
        
        public class SolutionSelection : HtmlConsoleTemplateBuilder
        {
            public string Name { get; set; }

            public string Version { get; set; }

            public int NuspecFileCount { get; set; }

            public override HtmlElement Build(LogEntry log) =>
                HtmlElement
                    .Builder
                    .span(span => span
                        .text("Current solution:")
                        .text(" ")
                        .span(s => s.text($"'{Name}'").color(Yellow))
                        .text(" ")
                        .span(s => s.text($"v{Version}").color(Magenta))
                        .text(" ")
                        .text($"({NuspecFileCount} package{(NuspecFileCount == 1 ? string.Empty : "s")})"));
        }
    }

    

    public class SelectSolution : HtmlConsoleTemplateBuilder
    {
        public override HtmlElement Build(LogEntry log) =>
            HtmlElement
                .Builder
                .span(span => span
                    .Indent(width: 1)
                    .text("Use the 'select' command to pick a solution."));
    }

    public class PackageInfo : HtmlConsoleTemplateBuilder
    {
        public string PackageId { get; set; }

        public int DependencyCount { get; set; }

        public override HtmlElement Build(LogEntry log) =>
            HtmlElement
                .Builder
                .span(x => x
                    .Indent(width: 1)
                    .text($"{PackageId} ")
                    .span(s => s.text($"({DependencyCount})").color(Magenta)));
    }

    public class PackageDependencySection : HtmlConsoleTemplateBuilder
    {
        public string Name { get; set; }

        public override HtmlElement Build(LogEntry log) =>
            HtmlElement
                .Builder
                .span(x => x
                    .Indent(width: 1, depth: 2)
                    .span(s => s.text($"[{Name}]").color(DarkGray)));
    }

    public class PackageDependencyInfo : HtmlConsoleTemplateBuilder
    {
        public string Name { get; set; }

        public string Version { get; set; }

        public override HtmlElement Build(LogEntry log) =>
            HtmlElement
                .Builder
                .span(x => x
                    .Indent(width: 1, depth: 2)
                    .text($"- {Name} ")
                    .span(s => s.text($"v{Version}").color(DarkGray)));
    }

    public class NuGetPackResultError : HtmlConsoleTemplateBuilder
    {
        public int ErrorCount { get; set; }

        public override HtmlElement Build(LogEntry log) =>
            HtmlElement
                .Builder
                .span(x => x
                    .Indent(width: 1)
                    .span(s => s
                        .text($"{ErrorCount} error(s) occured.")
                        .color(Red)));
    }

    public class NuGetPackResultSuccess : HtmlConsoleTemplateBuilder
    {
        public override HtmlElement Build(LogEntry log) =>
            HtmlElement
                .Builder
                .span(x => x
                    .Indent(width: 1)
                    .span(s => s.text("All packages successfully created.").color(Green)));
    }

    public class NuGetCommandStopwatch : HtmlConsoleTemplateBuilder
    {
        public TimeSpan Elapsed { get; set; }

        public int ThreadId { get; set; }

        public override HtmlElement Build(LogEntry log) =>
            HtmlElement
                .Builder
                .span(x => x
                    .Indent(width: 1)
                    .text($"Elapsed: {Elapsed.TotalSeconds:F1} sec [{ThreadId}]"));
    }

    public class NuGetCommandOutput : HtmlConsoleTemplateBuilder
    {
        public string Text { get; set; }

        public override HtmlElement Build(LogEntry log) =>
            HtmlElement
                .Builder
                .span(x => x
                    .Indent(width: 1)
                    .text(Text));
    }

    public class NuGetCommandError : HtmlConsoleTemplateBuilder
    {
        public string Text { get; set; }

        public override HtmlElement Build(LogEntry log) =>
            HtmlElement
                .Builder
                .span(x => x
                    .Indent(width: 1)
                    .text(Text)).color(Red);
    }

    public class NuGetPackError : HtmlConsoleTemplateBuilder
    {
        public string PackageId { get; set; }

        public override HtmlElement Build(LogEntry log) =>
            HtmlElement
                .Builder
                .span(x => x
                    .Indent(width: 1)
                    .text($"Could not create package: {PackageId}")).color(Red);
    }

    public class NuGetPushResult : HtmlConsoleTemplateBuilder
    {
        public int TotalCount { get; set; }

        public int SuccessfulCount { get; set; }

        public bool Success => SuccessfulCount == TotalCount;

        public override HtmlElement Build(LogEntry log) =>
            HtmlElement
                .Builder
                .span(x => x
                    .Indent(width: 1)
                    // Pushed 4/5 package(s)
                    .text($"Pushed {SuccessfulCount}/{TotalCount} package(s)")).color(Success ? Green : Red);
    }

    namespace Select
    {
        public class Response : HtmlConsoleTemplateBuilder
        {
            public string SolutionName { get; set; }

            public override HtmlElement Build(LogEntry log) =>
                HtmlElement
                    .Builder
                    .span(x => x.text("Selected ").span(s => s.text($"'{SolutionName}'").color(Yellow)));
        }
    }

    namespace Version
    {
        public class Response : HtmlConsoleTemplateBuilder
        {
            public string NewVersion { get; set; }

            public override HtmlElement Build(LogEntry log) =>
                HtmlElement
                    .Builder
                    .span(x => x
                        .Indent(width: 1)
                        // Pushed 4/5 package(s)
                        .text($"New version: v{NewVersion}"));
        }
    }

    internal static class HtmlElementExtensions
    {
        public static HtmlElement Indent(this HtmlElement element, int width = 3, int depth = 1)
        {
            return element.span(s => s.text(new string(' ', width * depth)));
        }
    }
}