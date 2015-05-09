using MargieBot.Models;

namespace MargieBot.MessageProcessors
{
    public interface IResponseProcessor
    {
        bool CanRespond(ResponseContext context);
        BotMessage GetResponse(ResponseContext context);
    }
}