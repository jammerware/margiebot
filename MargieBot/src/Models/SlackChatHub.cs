namespace MargieBot
{
    /// <summary>
    /// This represents a place in Slack where people can chat - typically a channel, group, or DM.
    /// </summary>
    public class SlackChatHub
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public SlackChatHubType Type { get; set; }

        public static SlackChatHub FromID(string hubID)
        {
            if (!string.IsNullOrEmpty(hubID)) {
                SlackChatHubType? hubType = null;

                switch (hubID.ToCharArray()[0]) {
                    case 'C': 
                        hubType = SlackChatHubType.Channel;
                        break;
                    case 'D': 
                        hubType = SlackChatHubType.DM;
                        break;
                    case 'G': 
                        hubType = SlackChatHubType.Group;
                        break;
                }

                if (hubType != null) {
                    return new SlackChatHub() {
                        ID = hubID,
                        Name = hubID,
                        Type = hubType.Value
                    };
                }
            }

            return null;
        }
    }
}