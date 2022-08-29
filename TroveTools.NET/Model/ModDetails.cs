using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace TroveTools.NET.Model
{
    /// <summary>
    /// Mod Details class for use in creating and saving YAML files while generating TMOD format mods
    /// </summary>
    public class ModDetails
    {
        public string Author { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public string PreviewPath { get; set; } = string.Empty;
        public List<string> Files { get; set; } = new List<string>();
        public List<string> Tags { get; set; } = new List<string>();

        public static ModDetails LoadFromYaml(string yamlContents)
        {
            var deserializer = new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
            return deserializer.Deserialize<ModDetails>(yamlContents);
        }

        public string GetYaml()
        {
            var serializer = new SerializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
            return serializer.Serialize(this);
        }

        public void SaveYamlFile(string yamlPath)
        {
            string yaml = GetYaml();
            using (StreamWriter sw = new StreamWriter(yamlPath, false))
            {
                sw.WriteLine("---");
                sw.Write(yaml);
                sw.WriteLine("...");
            }
        }
    }
}
