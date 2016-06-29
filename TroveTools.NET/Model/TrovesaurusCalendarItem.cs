using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TroveTools.NET.Converter;

namespace TroveTools.NET.Model
{
    class TrovesaurusCalendarItem
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("startdate")]
        public string StartDate { get; set; }

        [JsonProperty("enddate")]
        public string EndDate { get; set; }

        [JsonIgnore]
        public DateTime StartDateTime
        {
            get { return UnixTimeSecondsToDateTimeConverter.GetLocalDateTime(StartDate); }
        }

        [JsonIgnore]
        public DateTime EndDateTime
        {
            get { return UnixTimeSecondsToDateTimeConverter.GetLocalDateTime(EndDate); }
        }
    }
}
