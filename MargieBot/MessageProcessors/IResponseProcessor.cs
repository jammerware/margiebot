using MargieBot.Models;
namespace MargieBot.MessageProcessors
{
    public interface IResponseProcessor
    {
        bool CanRespond(MargieContext context);
        string GetResponse(MargieContext context);
    }
}