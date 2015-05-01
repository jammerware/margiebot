using MargieBot.MessageProcessors;
using MargieBot.Models;
using System.Text.RegularExpressions;

namespace MargieBot.UI.Infrastructure.BotResponseProcessors
{
    public class DefaultMessageProcessor : IResponseProcessor
    {
        public bool CanRespond(MargieContext context)
        {
            return
                Regex.IsMatch(context.Message.Text, @"\b(hi|hey|hello)\b", RegexOptions.IgnoreCase) &&
                !context.MessageHasBeenRespondedTo &&
                context.Message.User != context.MargiesUserID &&
                context.Message.User != Constants.USER_SLACKBOT;
        }

        public string GetResponse(MargieContext context)
        {
            return context.Phrasebook.GetQuery();
        }

        public bool ResponseRequiresBotMention(MargieContext context)
        {
            return true;
        }
    }
}