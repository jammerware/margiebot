using MargieBot.Infrastructure.Models;
using System.Text.RegularExpressions;

namespace MargieBot.Infrastructure.MessageProcessors
{
    public class DefaultMessageProcessor : IResponseProcessor
    {
        public bool CanRespond(MargieContext context)
        {
            return
                Regex.IsMatch(context.Message.Text, @"(hi|hey|hello)(.+)?(margie|margie\sbot|<@" + context.MargiesUserID + @">)", RegexOptions.IgnoreCase) &&
                !context.MessageHasBeenRespondedTo &&
                context.Message.User != context.MargiesUserID &&
                context.Message.User != Constants.USER_SLACKBOT;
        }

        public string GetResponse(MargieContext context)
        {
            return context.Phrasebook.GetQuery();
        }
    }
}