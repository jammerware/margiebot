using MargieBot.Infrastructure.Debugging;
using MargieBot.Infrastructure.Models;
using System.Text.RegularExpressions;

namespace MargieBot.Infrastructure.MessageProcessors
{
    public class DebugMessageProcessor : IMessageProcessor
    {
        public event MargieDebuggingEventHandler OnDebugRequested;

        public bool IsRelevant(SlackMessage message, Margie margie, bool hasBeenRespondedTo)
        {
            return Regex.IsMatch(message.Text, "margie(.+)?repeat");
        }

        public void Respond(SlackMessage message, Margie margie)
        {
            if (OnDebugRequested != null) {
                OnDebugRequested(message.Text, message.RawData);
            }
        }
    }
}