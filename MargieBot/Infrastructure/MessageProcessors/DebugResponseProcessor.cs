using System.Text.RegularExpressions;
using MargieBot.Infrastructure.EventHandlers;
using MargieBot.Infrastructure.Models;

namespace MargieBot.Infrastructure.MessageProcessors
{
    public class DebugResponseProcessor : IResponseProcessor
    {
        public event MargieDebuggingEventHandler OnDebugRequested;

        public bool CanRespond(MargieContext context)
        {
            return Regex.IsMatch(context.Message.Text, context.MargieNameRegex + "(.+)?debug");
        }

        public string GetResponse(MargieContext context)
        {
            if (OnDebugRequested != null) {
                OnDebugRequested(context.Message.RawData);
            }

            return "I'll send that right out to the debug winda, " + context.Message.FormattedUser + ". Hoo, boy. I hate for you to see me like this.";
        }
    }
}