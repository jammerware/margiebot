using Newtonsoft.Json;

namespace MargieBot.UI.Infrastructure.Models.GW2
{
    public class WvWMap
    {
        [JsonProperty(PropertyName = "scores")]
        public int[] Scores { get; set; }

        [JsonProperty(PropertyName = "type")]
        public WvWMapType Type { get; set; }
    }
}