using MargieBot.Infrastructure.Models;

namespace MargieBot.Infrastructure.MessageProcessors
{
    public class DefaultMessageProcessor : IMessageProcessor
    {
        public bool IsRelevant(SlackMessage message, Margie margie, bool hasBeenRespondedTo)
        {
            return (message.Text.ToLower().Contains("margie") || message.Text.Contains("@" +  margie.UserID))  && !hasBeenRespondedTo;
        }

        public void Respond(SlackMessage message, Margie margie)
        {
            margie.Say(margie.GetQuery(), message.Channel);
        }
    }
}