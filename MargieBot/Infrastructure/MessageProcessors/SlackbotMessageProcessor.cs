using MargieBot.Infrastructure.Models;
using System;

namespace MargieBot.Infrastructure.MessageProcessors
{
    public class SlackbotMessageProcessor : IMessageProcessor
    {
        public bool IsRelevant(SlackMessage message, Margie margie, bool hasBeenRespondedTo)
        {
            return (message.User == "USLACKBOT" && new Random().Next(8) <= 2);
        }

        public void Respond(SlackMessage message, Margie margie)
        {
            margie.Say(margie.GetSlackbotSalutation(), message.Channel);
        }
    }
}