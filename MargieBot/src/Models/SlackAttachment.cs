using System.Collections.Generic;
using Newtonsoft.Json;

namespace MargieBot
{
    public class SlackAttachment
    {
        /// <summary>
        /// Used to color the border along the left side of the message attachment.
        /// </summary>
        [JsonProperty(PropertyName = "color")]
        public string ColorHex { get; set; }

        /// <summary>
        /// A plain-text summary of the attachment. This text will be used in clients that don't show formatted text (e.g. IRC, mobile notifications).
        /// </summary>
        [JsonProperty(PropertyName = "fallback")]
        public string Fallback { get; set; }

        /// <summary>
        /// Attachment fields will be displayed in a table inside the message attachment.
        /// </summary>
        [JsonProperty(PropertyName = "fields")]
        public IList<SlackAttachmentField> Fields { get; set; }

        /// <summary>
        /// Enables text formatting for the value property of each field in <see cref="Fields"/>.
        /// </summary>
        public bool FieldsFormattingEnabled;

        /// <summary>
        /// A URL to an image file that will be displayed inside a message attachment.
        /// Large images will be resized to a maximum width of 400px or a maximum height of 500px, while still maintaining the original aspect ratio.
        /// </summary>
        [JsonProperty(PropertyName = "image_url")]
        public string ImageUrl { get; set; }

        /// <summary>
        /// A URL to an image file that will be displayed as a thumbnail on the right side of a message attachment.
        /// The thumbnail's longest dimension will be scaled down to 75px while maintaining the aspect ratio of the image.
        /// </summary>
        [JsonProperty(PropertyName = "thumb_url")]
        public string ThumbUrl { get; set; }

        /// <summary>
        /// Appears above the message attachment block.
        /// </summary>
        [JsonProperty(PropertyName = "pretext")]
        public string PreText { get; set; }

        /// <summary>
        /// Enables text formatting for the <see cref="PreText"/> property.
        /// </summary>
        public bool PreTextFormattingEnabled;

        /// <summary>
        /// The main text in a message attachment. The content will automatically collapse if it contains 700+ characters or 5+ linebreaks, and will display a "Show more..." link to expand the content.
        /// </summary>
        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }

        /// <summary>
        /// Enables text formatting for the <see cref="Text"/> property.
        /// </summary>
        public bool TextFormattingEnabled;

        /// <summary>
        /// Displayed as larger, bold text near the top of a message attachment
        /// </summary>
        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        /// <summary>
        /// A URL to hyperlink the <see cref="Title"/> property.
        /// </summary>
        [JsonProperty(PropertyName = "title_link")]
        public string TitleLink { get; set; }

        /// <summary>
        /// Small text used to display the author's name.
        /// </summary>
        [JsonProperty(PropertyName = "author_name")]
        public string AuthorName { get; set; }

        /// <summary>
        /// A URL to hyperlink the <see cref="AuthorName"/> property.
        /// </summary>
        [JsonProperty(PropertyName = "author_link")]
        public string AuthorLink { get; set; }

        /// <summary>
        /// A URL to an image file (16x16px) that will be displayed to the left of the <see cref="AuthorName"/> text.
        /// Requires <see cref="AuthorName"/> to be present.
        /// </summary>
        [JsonProperty(PropertyName = "author_icon")]
        public string AuthorIcon { get; set; }

        [JsonProperty(PropertyName = "mrkdwn_in")]
        protected IList<string> MarkdownIn
        {
            get
            {
                var enabled = new List<string>();

                if (PreTextFormattingEnabled)
                    enabled.Add("pretext");
                if (TextFormattingEnabled)
                    enabled.Add("text");
                if (FieldsFormattingEnabled)
                    enabled.Add("fields");

                return enabled;
            }
        } 

        public SlackAttachment()
        {
            Fields = new List<SlackAttachmentField>();
        }
    }
}