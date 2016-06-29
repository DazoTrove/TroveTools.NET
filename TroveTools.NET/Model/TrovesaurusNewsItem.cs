using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TroveTools.NET.Converter;

namespace TroveTools.NET.Model
{
    class TrovesaurusNewsItem
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("preview")]
        public string PreviewHtml { get; set; }

        [JsonProperty("image")]
        public string ImagePath { get; set; }

        [JsonProperty("author")]
        public string Author { get; set; }

        [JsonProperty("date")]
        public string UnixDate { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("views")]
        public string Views { get; set; }

        [JsonProperty("comments")]
        public string Comments { get; set; }

        [JsonProperty("tags")]
        public List<string> Tags { get; set; }

        [JsonIgnore]
        public DateTime Date
        {
            get { return UnixTimeSecondsToDateTimeConverter.GetLocalDateTime(UnixDate); }
        }
    }
}
