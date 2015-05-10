using System.Collections.Generic;
using Newtonsoft.Json;

namespace MargieBot.Models
{
    public class SlackAttachment
    {
        [JsonProperty(PropertyName = "color")]
        public string ColorHex { get; set; }

        [JsonProperty(PropertyName = "fallback")]
        public string Fallback { get; set; }

        [JsonProperty(PropertyName = "fields")]
        public IList<SlackAttachmentField> Fields { get; set; }

        [JsonProperty(PropertyName = "image_url")]
        public string ImageUrl { get; set; }

        [JsonProperty(PropertyName = "pretext")]
        public string PreText { get; set; }

        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }

        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        [JsonProperty(PropertyName = "title_link")]
        public string TitleLink { get; set; }

        public SlackAttachment()
        {
            Fields = new List<SlackAttachmentField>();
        }
    }
}