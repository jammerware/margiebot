using MargieBot.Infrastructure.Models;

namespace MargieBot.Infrastructure.MessageProcessors
{
    public class DefaultMessageProcessor : IResponseProcessor
    {
        public bool CanRespond(MargieContext context)
        {
            return 
                (context.Message.Text.ToLower().Contains("margie") || context.Message.Text.Contains("@" +  context.MargiesUserID))  && 
                !context.MessageHasBeenRespondedTo &&
                context.Message.User != context.MargiesUserID;
        }

        public string GetResponse(MargieContext context)
        {
            return context.Phrasebook.GetQuery();
        }
    }
}