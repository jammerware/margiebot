using System.Collections.Generic;

namespace MargieBot.Models
{
    public class ResponseContext
    {
        public bool BotHasResponded { get; set; }
        public string BotUserID { get; set; }
        public SlackMessage Message { get; set; }
        public Phrasebook Phrasebook { get; set; }
        public ScoreContext ScoreContext { get; set; }
        public IReadOnlyDictionary<string, string> UserNameCache { get; set; }
    }
}