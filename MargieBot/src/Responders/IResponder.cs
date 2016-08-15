namespace MargieBot
{
    public interface IResponder
    {
        bool CanRespond(ResponseContext context);
        BotMessage GetResponse(ResponseContext context);
    }
}