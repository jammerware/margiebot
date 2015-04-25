using MargieBot.Infrastructure.Models;

namespace MargieBot.Infrastructure.MessageProcessors
{
    public class DefaultMessageProcessor : IResponseProcessor
    {
        public bool CanResponse(MargieContext context)
        {
            return (context.Message.Text.ToLower().Contains("margie") || context.Message.Text.Contains("@" +  context.MargiesUserID))  && !context.MessageHasBeenRespondedTo;
        }

        public string Respond(MargieContext context)
        {
            return context.Phrasebook.GetQuery();
        }
    }
}