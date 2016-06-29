using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TroveTools.NET.Converter;

namespace TroveTools.NET.Model
{
    class TroveServerStatus
    {
        public class LiveStatus
        {
            [JsonProperty("online"), JsonConverter(typeof(JsonBoolConverter))]
            public bool Online { get; set; }

            [JsonProperty("date")]
            public string Date { get; set; }

            [JsonIgnore]
            public DateTime DateTime
            {
                get { return UnixTimeSecondsToDateTimeConverter.GetLocalDateTime(Date); }
            }
        }

        public class PublicTestServerStatus
        {
            [JsonProperty("online"), JsonConverter(typeof(JsonBoolConverter))]
            public bool Online { get; set; }

            [JsonProperty("date")]
            public string Date { get; set; }

            [JsonIgnore]
            public DateTime DateTime
            {
                get { return UnixTimeSecondsToDateTimeConverter.GetLocalDateTime(Date); }
            }
        }

        public LiveStatus Live { get; set; }

        public PublicTestServerStatus PTS { get; set; }
    }
}
