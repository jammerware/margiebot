using System.Collections.Generic;

namespace MargieBot.Infrastructure.Models
{
    public class MargieContext
    {
        public string MargiesUserID { get; set; }
        public SlackMessage Message { get; set; }
        public bool MessageHasBeenRespondedTo { get; set; }
        public Phrasebook Phrasebook { get; set; }
        public ScoreContext ScoreContext { get; set; }
        public IReadOnlyDictionary<string, string> UserNameCache { get; set; }

        public string MargieNameRegex
        {
            get { return "(margie|margie bot|<@" + MargiesUserID + ">)"; }
        }
    }
}