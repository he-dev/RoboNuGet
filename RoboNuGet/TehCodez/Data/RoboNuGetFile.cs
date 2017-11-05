using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Reusable;

namespace RoboNuGet.Data
{
    internal class RoboNuGetFile
    {
        private static readonly JsonSerializerSettings DefaultSerializerSettings = new JsonSerializerSettings
        {
            Converters = {new JsonOverridableConverter()},
            Formatting = Formatting.Indented
        };

        private const string DefaultFileName = "RoboNuGet.json";


        public Overrideable<string> SolutionFileName { get; set; }

        public string PackageVersion { get; set; }

        public string[] TargetFrameworkVersions { get; set; }

        public bool IsPrerelease { get; set; }

        public MsBuild MsBuild { get; set; }

        public NuGetSection NuGet { get; set; }

        // Computed properties

        [JsonIgnore]
        public string FullVersion => IsPrerelease ? $"{PackageVersion}-pre" : PackageVersion;

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

        public void IncrementPatchVersion()
        {
            PackageVersion = Regex.Replace(PackageVersion, @"^(?<major>\d+)\.(?<minor>\d+)\.(?<patch>\d+)", m =>
                                               $"{m.Groups["major"].Value}." +
                                               $"{m.Groups["minor"]}." +
                                               $"{int.Parse(m.Groups["patch"].Value) + 1}");
        }
    }

    [UsedImplicitly]
    internal class NuGetSection
    {
        public string OutputDirectoryName { get; set; }

        public string NuGetConfigName { get; set; }

        public Dictionary<SoftString, string> Commands { get; set; }
    }

    [PublicAPI]
    public class Overrideable<T>
    {
        private readonly T _value;

        [JsonConstructor]
        public Overrideable(T value)
        {
            _value = value;
            Value = value;
        }

        public T Value
        {
            get => _value;
            set => Current = value;
        }

        [JsonIgnore]
        public T Current { get; private set; }

        public static implicit operator T(Overrideable<T> overridable) => overridable.Current;
    }

    public class JsonOverridableConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            var genericArguments = objectType.GetGenericArguments();
            if (genericArguments.Length == 1)
            {
                var overridable = typeof(Overrideable<>).MakeGenericType(genericArguments[0]);
                return (objectType == overridable);
            }
            return false;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            // .Single is safe here because CanConvert will prevent invalid types.
            var overridableGeneringArgument = objectType.GetGenericArguments().Single();
            var value = serializer.Deserialize(reader, overridableGeneringArgument);
            return Activator.CreateInstance(objectType, new[] {value});
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            // .Single is safe here because CanConvert will prevent invalid types.
            var valueType = value.GetType().GetGenericArguments().Single();
            var actualValue = value.GetType().GetProperty(nameof(Overrideable<object>.Value)).GetValue(value);
            serializer.Serialize(writer, actualValue);
        }
    }
}