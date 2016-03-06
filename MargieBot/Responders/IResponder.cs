using MargieBot.Models;

namespace MargieBot.Responders
{
    public interface IResponder
    {
        bool CanRespond(ResponseContext context);
        BotResponse GetResponse(ResponseContext context);
        BotResponseType GetResponseType();
    }
}