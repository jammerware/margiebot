using System;
using System.Reflection;
using System.Text.RegularExpressions;
using MargieBot.MessageProcessors;
using MargieBot.Models;

namespace MargieBot.UI.Infrastructure.BotResponseProcessors
{
    public class WhatsNewResponseProcessor : IResponseProcessor
    {
        public bool CanRespond(ResponseContext context)
        {
            return context.Message.MentionsBot && Regex.IsMatch(context.Message.Text, @"\b(what's new)\b", RegexOptions.IgnoreCase);
        }

        public BotMessage GetResponse(ResponseContext context)
        {
            Version version = Assembly.GetExecutingAssembly().GetName().Version;

            string message =
                @"I'm " + context.BotUserName + " v." +
                version.Major.ToString() + "." +
                version.Minor.ToString() + "." +
                version.Build.ToString() + "! Here's what all's been goin' on with me lately.```" +
                "- Scoring is fun, and y'all seem to like it as much as I do, so I'm a little less picky about it now. You can score more than one person at a time. Try it now, but make sure you include @ben. He's my favorite :)\n" +
                "- I'm an internet phenomenon now, y'all! You can learn more about me and how I work on github at https://github.com/jammerware/margiebot/wiki and even view my source! Y'all be gentlemen and ladies now. I'm a complicated gal!\n" +
                "```";

            return new BotMessage() { Text = message };
        }
    }
}