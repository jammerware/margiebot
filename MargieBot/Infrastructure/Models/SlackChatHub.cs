namespace MargieBot.Infrastructure.Models
{
    /// <summary>
    /// This represents a place in Slack where people can chat - typically a channel, group, or DM.
    /// </summary>
    public class SlackChatHub
    {
        public string Name { get; set; }
        public SlackChatHubType Type { get; set; }
    }
}