using MargieBot.Infrastructure.Models;
namespace MargieBot.Infrastructure.MessageProcessors
{
    public interface IMessageProcessor
    {
        bool IsRelevant(SlackMessage message, Margie margie, bool hasBeenRespondedTo);
        void Respond(SlackMessage message, Margie margie);
    }
}