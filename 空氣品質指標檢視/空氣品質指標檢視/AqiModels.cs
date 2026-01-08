using System.Collections.Generic;
using Newtonsoft.Json;

namespace YourNamespace.Models
{
    public class AqiApiResponse
    {
        [JsonProperty("records")]
        public List<AqiRecord> Records { get; set; } = new();
    }

    public class AqiRecord
    {
        [JsonProperty("sitename")]
        public string SiteName { get; set; }

        [JsonProperty("county")]
        public string County { get; set; }

        [JsonProperty("aqi")]
        public string AQI { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("pm2.5")]
        public string PM25 { get; set; }

        [JsonProperty("pm10")]
        public string PM10 { get; set; }

        [JsonProperty("o3")]
        public string O3 { get; set; }

        [JsonProperty("co")]
        public string CO { get; set; }

        [JsonProperty("so2")]
        public string SO2 { get; set; }

        [JsonProperty("no2")]
        public string NO2 { get; set; }

        [JsonProperty("publishtime")]
        public string PublishTime { get; set; }

        [JsonProperty("importdate")]
        public string ImportDate { get; set; }
    }
}
