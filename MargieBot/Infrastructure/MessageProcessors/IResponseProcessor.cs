using MargieBot.Infrastructure.Models;
namespace MargieBot.Infrastructure.MessageProcessors
{
    public interface IResponseProcessor
    {
        bool CanRespond(MargieContext context);
        string GetResponse(MargieContext context);
    }
}