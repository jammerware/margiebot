using MargieBot.Models;

namespace MargieBot.MessageProcessors
{
    public interface IResponseProcessor
    {
        bool CanRespond(ResponseContext context);
        string GetResponse(ResponseContext context);
    }
}