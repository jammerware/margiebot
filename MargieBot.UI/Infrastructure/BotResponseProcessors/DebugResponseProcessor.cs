using System.Text.RegularExpressions;
using MargieBot.EventHandlers;
using MargieBot.MessageProcessors;
using MargieBot.Models;

namespace MargieBot.UI.Infrastructure.BotResponseProcessors
{
    public class DebugResponseProcessor : IResponseProcessor
    {
        public event MargieDebuggingEventHandler OnDebugRequested;

        public bool CanRespond(ResponseContext context)
        {
            return context.Message.MentionsBot && Regex.IsMatch(context.Message.Text, @"\bdebug\b", RegexOptions.IgnoreCase);
        }

        public BotMessage GetResponse(ResponseContext context)
        {
            if (OnDebugRequested != null) {
                OnDebugRequested(context.Message.RawData);
            }

            return new BotMessage() {
                Text = "I'll send that right out to the debug winda, " + context.Message.User.FormattedUserID + ". Hoo, boy. I hate for y'all to see me like this."
            };
        }
    }
}