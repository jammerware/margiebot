namespace MargieBot.Infrastructure.Models
{
    public class SlackMessage
    {
        public string Channel { get; set; }
        public string RawData { get; set; }
        public string Text { get; set; }
        public string User { get; set; }
    }
}