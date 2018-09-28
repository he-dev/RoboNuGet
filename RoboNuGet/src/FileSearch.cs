using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Custom;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.IO;
using RoboNuGet.Files;

namespace RoboNuGet
{
    internal interface IFileSearch
    {
        [NotNull]
        string FindSolutionFile();

        [NotNull, ItemNotNull]
        IEnumerable<NuspecFile> FindNuspecFiles();
    }

    [UsedImplicitly]
    internal class FileSearch : IFileSearch
    {
        private readonly IFileSystem _fileSystem;
        private readonly RoboNuGetFile _roboNuGetFile;

        public FileSearch(IFileSystem fileSystem, RoboNuGetFile roboNuGetFile)
        {
            _fileSystem = fileSystem;
            _roboNuGetFile = roboNuGetFile;
        }

        public string FindSolutionFile()
        {
            var solutionFileName = _roboNuGetFile.SolutionFileName;

            if (Path.IsPathRooted(solutionFileName))
            {
                return solutionFileName;
            }
            else
            {

                return Path.Combine(
                    Directory.GetParent(
                        Path.GetDirectoryName(
                            Process.GetCurrentProcess().MainModule.FileName)
                        ).FullName, 
                    solutionFileName
                );
            }

            //var solutionFileFilter = FileFilterFactory.Default.Create(_roboNuGetFile.SolutionFileName);
            //var files =
            //    _fileSystem
            //        .EnumerateDirectories(Directory.GetCurrentDirectory(), CreateExcludePredicate())
            //        .SelectMany(_fileSystem.EnumerateFiles)
            //        .Where(solutionFileFilter)
            //        .ToList();

            //const int notFound = 0;
            //const int exactMatch = 1;

            //switch (files.Count)
            //{
            //    case notFound:
            //        throw DynamicException.Factory.CreateDynamicException(
            //            $"SolutionFileNotFound{nameof(Exception)}",
            //            $"Solution file {_roboNuGetFile.SolutionFileName.QuoteWith("'")} not found in {_roboNuGetFile.SolutionDirectoryName.QuoteWith("'")}.", null);

            //    case exactMatch:
            //        return files.Single();

            //    default:
            //        throw DynamicException.Factory.CreateDynamicException(
            //            $"MultipleSolutionFiles{nameof(Exception)}",
            //            $"Multiple solution files {_roboNuGetFile.SolutionFileName.QuoteWith("'")} found in {_roboNuGetFile.SolutionDirectoryName.QuoteWith("'")}.", null);
            //}
        }

        public IEnumerable<NuspecFile> FindNuspecFiles()
        {
            var nuspecPattern = Pattern.FromWildcard($"*{NuspecFile.Extension}");
            var solutionDirectoryName = Path.GetDirectoryName(FindSolutionFile());

            var nuspecFileNames =
                _fileSystem
                    .EnumerateDirectories(solutionDirectoryName, CreateExcludePredicate())
                    .SelectMany(_fileSystem.EnumerateFiles)
                    .Where(name => nuspecPattern.Matches(name));

            // Searching for *.nuspec only one level deep.
            foreach (var nuspecFileName in nuspecFileNames)
            {
                yield return NuspecFile.Load(nuspecFileName);
            }
        }

        private Func<string, bool> CreateExcludePredicate()
        {
            var patterns =
                (from name in _roboNuGetFile.ExcludeDirectories
                 select Pattern.FromWildcard(name)).ToList();

            return input => patterns.Any(pattern => pattern.Matches(input));
        }
    }

    [DebuggerDisplay("{ToString(),nq}")]
    public class Pattern
    {
        private readonly Regex _matcher;

        private Pattern(Regex matcher)
        {
            _matcher = matcher;
        }

        public static Func<string, bool> None => _ => false;

        public static Func<string, bool> Any => _ => true;

        [NotNull]
        public static Pattern FromRegex([NotNull] string pattern, bool ignoreCase = true)
        {
            if (pattern == null) throw new ArgumentNullException(nameof(pattern));

            var options = RegexOptions.Compiled;
            if (ignoreCase)
            {
                options |= RegexOptions.IgnoreCase;
            }
            return new Regex(pattern, options);
        }

        [NotNull]
        public static Pattern FromWildcard([NotNull] string pattern, bool ignoreCase = true)
        {
            if (pattern == null) throw new ArgumentNullException(nameof(pattern));

            pattern = Regex.Replace(pattern, @"\.", @"\.");
            pattern = Regex.Replace(pattern, @"\?", @".");
            pattern = Regex.Replace(pattern, @"\*", $@".*?");

            return FromRegex($"^{pattern}$", ignoreCase);
        }

        public bool Matches(string value) => _matcher.IsMatch(value);

        public override string ToString() => _matcher.ToString();

        public static implicit operator Func<string, bool>(Pattern pattern)
        {
            return pattern is null ? default(Func<string, bool>) : pattern._matcher.IsMatch;
        }

        public static implicit operator Pattern(Regex matcher)
        {
            return matcher is null ? null : new Pattern(matcher);
        }
    }
}