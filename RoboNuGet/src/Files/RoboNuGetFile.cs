using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable;
using Reusable.Reflection;
using RoboNuGet.Commands;

namespace RoboNuGet.Files
{
    internal class SolutionInfo
    {
        //[DefaultValue("*.sln")]
        [JsonRequired]
        public string FileName { get; set; }

        [JsonIgnore]
        public string DirectoryName => Path.GetDirectoryName(FileName);

        [JsonRequired]
        public string PackageVersion { get; set; }

        public bool IsPrerelease { get; set; }

        [JsonIgnore]
        public string FullVersion => $"{PackageVersion}{(IsPrerelease ? "-pre" : string.Empty)}";
        
        //public IEnumerable<NuspecFile> NuspecFiles => _directoryTree.WalkSilently(DirectoryName).
    }

    [UsedImplicitly]
    internal class RoboNuGetFile
    {
        private static readonly JsonSerializerSettings DefaultSerializerSettings = new JsonSerializerSettings
        {
            //Converters = {new JsonOverridableConverter()},
            Formatting = Formatting.Indented,
            DefaultValueHandling = DefaultValueHandling.Populate
        };

        private const string DefaultFileName = "RoboNuGet.json";

        public string[] ExcludeDirectories { get; set; }
        
        [JsonRequired]
        public IEnumerable<SolutionInfo> Solutions { get; set; }

        public MsBuild MsBuild { get; set; }

        public NuGetSection NuGet { get; set; }

        // Computed properties

        [JsonIgnore]
        public static string FileName
        {
            get
            {
                var currentDirectory = Directory.GetCurrentDirectory();
                var fileName = Path.Combine(currentDirectory, DefaultFileName);
                return fileName;
            }
        }
        
        [JsonIgnore]
        internal SolutionInfo SelectedSolution { get; set; }

        public static RoboNuGetFile Load()
        {
            var json = File.ReadAllText(FileName);
            var config = JsonConvert.DeserializeObject<RoboNuGetFile>(json, DefaultSerializerSettings);
            return config;
        }

        public void Save()
        {
            var json = JsonConvert.SerializeObject(this, DefaultSerializerSettings);
            File.WriteAllText(FileName, json);
        }
    }

    [UsedImplicitly]
    internal class NuGetSection
    {
        public string OutputDirectoryName { get; set; }

        public string NuGetConfigName { get; set; }

        public Dictionary<SoftString, string> Commands { get; set; }
    }

    //[PublicAPI]
    //public class Overrideable<T>
    //{
    //    private readonly T _value;

    //    [JsonConstructor]
    //    public Overrideable(T value)
    //    {
    //        _value = value;
    //        Value = value;
    //    }

    //    public T Value
    //    {
    //        get => _value;
    //        set => Current = value;
    //    }

    //    [JsonIgnore]
    //    public T Current { get; private set; }

    //    public static implicit operator T(Overrideable<T> overridable) => overridable.Current;
    //}

    //public class JsonOverridableConverter : JsonConverter
    //{
    //    public override bool CanConvert(Type objectType)
    //    {
    //        var genericArguments = objectType.GetGenericArguments();
    //        if (genericArguments.Length == 1)
    //        {
    //            var overridable = typeof(Overrideable<>).MakeGenericType(genericArguments[0]);
    //            return (objectType == overridable);
    //        }
    //        return false;
    //    }

    //    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    //    {
    //        // .Single is safe here because CanConvert will prevent invalid types.
    //        var overridableGeneringArgument = objectType.GetGenericArguments().Single();
    //        var value = serializer.Deserialize(reader, overridableGeneringArgument);
    //        return Activator.CreateInstance(objectType, new[] {value});
    //    }

    //    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    //    {
    //        // .Single is safe here because CanConvert will prevent invalid types.
    //        var valueType = value.GetType().GetGenericArguments().Single();
    //        var actualValue = value.GetType().GetProperty(nameof(Overrideable<object>.Value)).GetValue(value);
    //        serializer.Serialize(writer, actualValue);
    //    }
    //}
}