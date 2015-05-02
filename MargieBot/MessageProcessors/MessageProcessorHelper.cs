using MargieBot.Models;
using System;

namespace MargieBot.MessageProcessors
{
    public static class MessageProcessorHelper
    {
        public static IResponseProcessor Create(Func<MargieContext, bool> canRespond, Func<MargieContext, string> getResponse, bool requireBotMention = true)
        {
            return 
                requireBotMention ? 
                    new SimpleBotMentionedResponseProcessor() { CanRespondFactory = canRespond, GetResponseFactory = getResponse } : 
                    new SimpleResponseProcessor() { CanRespondFactory = canRespond, GetResponseFactory = getResponse };
        }
    }
}
