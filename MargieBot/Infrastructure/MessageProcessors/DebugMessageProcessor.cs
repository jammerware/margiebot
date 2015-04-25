using MargieBot.Infrastructure.Debugging;
using MargieBot.Infrastructure.Models;
using System.Text.RegularExpressions;

namespace MargieBot.Infrastructure.MessageProcessors
{
    public class DebugMessageProcessor : IResponseProcessor
    {
        public event MargieDebuggingEventHandler OnDebugRequested;

        public bool CanRespond(SlackMessage message, bool hasBeenRespondedTo)
        {
            return Regex.IsMatch(message.Text, "margie(.+)?repeat");
        }

        public string Respond(SlackMessage message, Phrasebook phrasebook)
        {
            if (OnDebugRequested != null) {
                OnDebugRequested(message.Text, message.RawData);
            }

            return "I'll send that right out to the debug winda, " + message.User + ". Hoo, boy. I hate for you to see me like this.";
        }
    }
}