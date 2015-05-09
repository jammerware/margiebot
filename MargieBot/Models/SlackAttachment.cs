using System.Collections.Generic;

namespace MargieBot.Models
{
    public class SlackAttachment
    {
        public string ColorHex { get; set; }
        public string Fallback { get; set; }
        public IList<SlackAttachmentField> Fields { get; private set; }
        public string Text { get; set; }
        public string Title { get; set; }
        public string TitleLink { get; set; }

        public SlackAttachment()
        {
            Fields = new List<SlackAttachmentField>();
        }
    }
}