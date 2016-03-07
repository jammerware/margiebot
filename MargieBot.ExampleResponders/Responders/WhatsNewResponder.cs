using System;
using System.Reflection;
using System.Text.RegularExpressions;
using MargieBot.Models;
using MargieBot.Responders;

namespace MargieBot.ExampleResponders.Responders
{
    public class WhatsNewResponder : IResponder
    {
        public bool CanRespond(ResponseContext context)
        {
            return (context.Message.MentionsBot || context.Message.ChatHub.Type == SlackChatHubType.DM) && Regex.IsMatch(context.Message.Text, @"\b(what's new)|(whats new)\b", RegexOptions.IgnoreCase);
        }

        public BotMessage GetResponse(ResponseContext context)
        {
            Version version = Assembly.GetExecutingAssembly().GetName().Version;

            string message =
                @"I'm " + context.BotUserName + " v." +
                version.Major.ToString() + "." +
                version.Minor.ToString() + "." +
                version.Build.ToString() + "! Here's what all's been goin' on with me lately.```" +
                "- I can tell you all the cool things I can do now. Ask me 'bout what I can do!\n" + 
                "- For any response that used to need you to mention me - well, they still do. But if you DM me, you don't have to. So if you're in devroom and want me to post the weather, you gotta say my name. But if you're just curious for your own self, you can just ask me privately without mentioning my name. I'll know what y'all mean.\n" +
                "- When I announce the score, I'll also tell ya when the next reset is coming. It's a tight race, boys and girls!\n" +
                "- My dice-rolling responder is a little more robust, since apparently y'all really just HAVE to roll 9000 9000-sided dice every now and then and get real confused when I can't do it.\n" +
                "- My internet legacy is growin', y'all! My wiki at https://github.com/jammerware/margiebot/wiki is real polished now, and you can add me to your next bot project from NuGet! Just Install-Package MargieBot.\n" +
                "```";

            return new BotMessage() { Text = message };
        }
    }
}