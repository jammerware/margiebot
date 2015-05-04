using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using MargieBot.MessageProcessors;
using MargieBot.Models;

// intentional put in the root namespace so that anyone using the Bot class will have these
namespace MargieBot
{
    public static class BotExtensions
    {
        public static IResponseProcessor CreateResponseProcessor(this Bot bot, Func<ResponseContext, bool> canRespond, Func<ResponseContext, string> getResponse)
        {
            return new SimpleResponseProcessor() { CanRespondFunction = canRespond, GetResponseFunctions = new List<Func<ResponseContext, string>>() { getResponse } };
        }

        public static MargieSimpleResponseChainer RespondsTo(this Bot bot, string phrase)
        {
            MargieSimpleResponseChainer chainer = new MargieSimpleResponseChainer();
            chainer.ResponseProcessor = new SimpleResponseProcessor();
            chainer.ResponseProcessor.CanRespondFunction = (ResponseContext context) => { return Regex.IsMatch(context.Message.Text, @"\b" + Regex.Escape(phrase) + @"\b", RegexOptions.IgnoreCase); };
            bot.ResponseProcessors.Add(chainer.ResponseProcessor);

            return chainer;
        }

        public class MargieSimpleResponseChainer
        {
            internal MargieSimpleResponseChainer() { }
            internal SimpleResponseProcessor ResponseProcessor { get; set; }

            public MargieSimpleResponseChainer With(string response)
            {
                this.ResponseProcessor.GetResponseFunctions.Add((ResponseContext context) => { return response; });
                return this;
            }

            public MargieSimpleResponseChainer IfBotIsMentioned()
            {
                Func<ResponseContext, bool> oldResponseCheck = this.ResponseProcessor.CanRespondFunction;
                this.ResponseProcessor.CanRespondFunction = (ResponseContext context) => { return oldResponseCheck(context) && context.Message.MentionsBot; };

                return this;
            }
        }
    }
}