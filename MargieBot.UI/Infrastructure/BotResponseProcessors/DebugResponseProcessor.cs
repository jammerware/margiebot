using MargieBot.EventHandlers;
using MargieBot.MessageProcessors;
using MargieBot.Models;
using System.Text.RegularExpressions;

namespace MargieBot.UI.Infrastructure.BotResponseProcessors
{
    public class DebugResponseProcessor : IBotMentionedResponseProcessor
    {
        public event MargieDebuggingEventHandler OnDebugRequested;

        public bool CanRespond(MargieContext context)
        {
            return Regex.IsMatch(context.Message.Text, @"\bdebug\b", RegexOptions.IgnoreCase);
        }

        public string GetResponse(MargieContext context)
        {
            if (OnDebugRequested != null) {
                OnDebugRequested(context.Message.RawData);
            }

            return "I'll send that right out to the debug winda, " + context.Message.FormattedUser + ". Hoo, boy. I hate for y'all to see me like this.";
        }
    }
}