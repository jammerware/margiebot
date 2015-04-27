using MargieBot.Infrastructure.Models;
using System;
using System.Reflection;
using System.Text.RegularExpressions;

namespace MargieBot.Infrastructure.MessageProcessors
{
    public class WhatsNewResponseProcessor : IResponseProcessor
    {
        public bool CanRespond(MargieContext context)
        {
            return Regex.IsMatch(context.Message.Text, "(.+)?what's new(.+)?", RegexOptions.IgnoreCase) && Regex.IsMatch(context.Message.Text, context.MargieNameRegex, RegexOptions.IgnoreCase);
        }

        public string GetResponse(MargieContext context)
        {
            Version version = Assembly.GetExecutingAssembly().GetName().Version;

            return
                @"I'm Margiebot v." +
                version.Major.ToString() + "." +
                version.Minor.ToString() + "." +
                version.Build.ToString() + "! Here's what all's been goin' on with me lately.```" +
                "- Made my default response processor a little less yammery.\n" +
                "- When you ask me 'What's new?', I can tell ya now!\n" +
                "- I can respond to things like 'What do you do, Margie?' now." + 
                "```";
        }
    }
}
