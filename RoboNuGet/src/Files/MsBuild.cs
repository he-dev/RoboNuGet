using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Extensions;

namespace RoboNuGet.Files
{
    [UsedImplicitly, PublicAPI]
    internal class MsBuild
    {
        public string Target { get; set; }

        public bool NoLogo { get; set; }

        public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();

        public Dictionary<string, string> Switches { get; set; } = new Dictionary<string, string>();

        public string ToString(string solutionFileName)
        {
            var arguments = new List<string>();

            if (Target.IsNotNullOrEmpty())
            {
                arguments.Add($"/target:{Target}");
            }

            if (NoLogo)
            {
                arguments.Add("/nologo");
            }

            if (Switches?.Any() == true)
            {
                arguments.AddRange(Switches.Select(x => $"/{x.Key}{(string.IsNullOrEmpty(x.Value) ? string.Empty : $":{x.Value}")}"));
            }

            if (Properties?.Any() == true)
            {
                arguments.AddRange(Properties.Select(property => $"/property:{property.Key}=\"{property.Value}\""));
            }

            arguments.Add(solutionFileName);

            return string.Join(" ", arguments);
        }
    }
}