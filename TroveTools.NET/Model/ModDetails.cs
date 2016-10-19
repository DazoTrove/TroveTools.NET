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
        public string Author { get; set; }
        public string Title { get; set; }
        public string Notes { get; set; }
        public string PreviewPath { get; set; }
        public List<string> Files { get; set; }

        public static ModDetails LoadFromYaml(string yamlContents)
        {
            var deserializer = new DeserializerBuilder().WithNamingConvention(new CamelCaseNamingConvention()).Build();
            return deserializer.Deserialize<ModDetails>(yamlContents);
        }

        public string GetYaml()
        {
            var serializer = new SerializerBuilder().WithNamingConvention(new CamelCaseNamingConvention()).Build();
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
