using System;
using System.Collections.Generic;
using MargieBot.Models;

namespace MargieBot.MessageProcessors
{
    public static class MessageProcessorHelper
    {
        public static IResponseProcessor Create(Func<MargieContext, bool> canRespond, Func<MargieContext, string> getResponse, bool requireBotMention = true)
        {
            return 
                requireBotMention ? 
                    new SimpleBotMentionedResponseProcessor() { CanRespondFunction = canRespond, GetResponseFunctions = new List<Func<MargieContext, string>>() { getResponse  } } :
                    new SimpleResponseProcessor() { CanRespondFunction = canRespond, GetResponseFunctions = new List<Func<MargieContext, string>>() { getResponse } };
        }
    }
}