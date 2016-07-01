using System.Collections.Generic;

namespace MargieBot.Models
{
    public class BotMessage
    {
        public IList<SlackAttachment> Attachments { get; set; }
        public SlackChatHub ChatHub { get; set; }
        public string Text { get; set; }

        public BotMessage()
        {
            Attachments = new List<SlackAttachment>(); 
        }
    }
}