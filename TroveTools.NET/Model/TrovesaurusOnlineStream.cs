using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TroveTools.NET.Model
{
    class TrovesaurusOnlineStream
    {
        public const string TwitchChannelUrl = "https://www.twitch.tv/{0}";


        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("channel")]
        public string Channel { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("online")]
        public string Online { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("preview")]
        public string PreviewImage { get; set; }

        [JsonProperty("viewers")]
        public int Viewers { get; set; }

        [JsonProperty("featured")]
        public string Featured { get; set; }

        [JsonProperty("updated")]
        public string Updated { get; set; }

        [JsonIgnore]
        public string Url
        {
            get { return string.Format(TwitchChannelUrl, Channel); }
        }
    }
}
