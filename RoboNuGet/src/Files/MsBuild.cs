using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Extensions;

namespace RoboNuGet.Files
{
    [UsedImplicitly, PublicAPI]
    internal class MsBuild
    {
        public string? Target { get; set; }

        public bool NoLogo { get; set; }

        public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();

        public Dictionary<string, string> Switches { get; set; } = new Dictionary<string, string>();

        public string RenderArgs(string solutionFileName)
        {
            var arguments = new List<string>
            {
                Target is {} ? $"/target:{Target}" : string.Empty,
                NoLogo ? "/nologo" : string.Empty
            };
            
            arguments.AddRange(Switches.Select(x => $"/{x.Key}{(string.IsNullOrEmpty(x.Value) ? string.Empty : $":{x.Value}")}"));
            arguments.AddRange(Properties.Select(property => $"/property:{property.Key}=\"{property.Value}\""));
            arguments.Add(solutionFileName);

            return string.Join(" ", arguments.Where(Conditional.IsNullOrEmpty));
        }
    }
}