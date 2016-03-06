namespace MargieBot.Models
{
    public class SlackMessage
    {
        public SlackChatHub ChatHub { get; set; }
        public bool MentionsBot { get; set; }
        public string RawData { get; set; }
        public string Text { get; set; }
        public SlackUser User { get; set; }
        public string SubType { get; set; }
        public bool Hidden { get; set; }
        public SlackMessage SubMessage { get; set; }
        public string Timestamp { get; set; }
    }
}