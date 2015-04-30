namespace MargieBot.Models
{
    /// <summary>
    /// This represents a place in Slack where people can chat - typically a channel, group, or DM.
    /// </summary>
    public class SlackChatHub
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public SlackChatHubType Type { get; set; }
    }
}