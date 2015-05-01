using MargieBot.MessageProcessors;
using MargieBot.Models;
using System;
using System.Reflection;
using System.Text.RegularExpressions;

namespace MargieBot.UI.Infrastructure.BotResponseProcessors
{
    public class WhatsNewResponseProcessor : IBotMentionedResponseProcessor
    {
        public bool CanRespond(MargieContext context)
        {
            return Regex.IsMatch(context.Message.Text, @"\b(what's new)\b", RegexOptions.IgnoreCase);
        }

        public string GetResponse(MargieContext context)
        {
            Version version = Assembly.GetExecutingAssembly().GetName().Version;

            return
                @"I'm Margiebot v." +
                version.Major.ToString() + "." +
                version.Minor.ToString() + "." +
                version.Build.ToString() + "! Here's what all's been goin' on with me lately.```" +
                "- I'm real stable now, y'all.\n" +
                "- Before I tended to get a touch ornery (or \"throw exceptions\" as them city folk say) when Slack did that \"unfurling\" business. It just rubbed me the wrong way. Now I'm okay with it though.\n" +
                "- I learned about the weather, y'all! Ask me now!" + 
                "```";
        }
    }
}
