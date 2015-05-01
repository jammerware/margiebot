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

        private class SimpleResponseProcessor : IResponseProcessor
        {
            public Func<MargieContext, bool> CanRespondFactory { get; set; }
            public Func<MargieContext, string> GetResponseFactory { get; set; }

            public bool CanRespond(MargieContext context)
            {
                return CanRespondFactory(context);
            }

            public string GetResponse(MargieContext context)
            {
                return GetResponseFactory(context);
            }
        }

        private class SimpleBotMentionedResponseProcessor : SimpleResponseProcessor, IBotMentionedResponseProcessor { }
    }
}
