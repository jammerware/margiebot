using System.Collections.Generic;
using System.Text;

namespace MargieBot.Utilities
{
    internal class BotNameRegexComposer
    {
        public string ComposeFor(string botName, string botUserID, IEnumerable<string> aliases)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append($@"(<@{botUserID}>|");
            builder.Append($@"\b{botName}\b");

            foreach (string alias in aliases) {
                builder.Append(@"|\b" + alias + @"\b");
            }
            builder.Append(@")");
            return builder.ToString();
        }
    }
}