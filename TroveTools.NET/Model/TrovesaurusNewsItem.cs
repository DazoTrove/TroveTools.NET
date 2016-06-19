using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
