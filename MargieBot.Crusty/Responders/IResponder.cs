using MargieBot.Models;

namespace MargieBot.Responders
{
    public interface IResponder
    {
        bool CanRespond(ResponseContext context);
        BotMessage GetResponse(ResponseContext context);
    }
}