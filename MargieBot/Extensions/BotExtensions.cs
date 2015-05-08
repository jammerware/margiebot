using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using MargieBot.MessageProcessors;
using MargieBot.Models;

// intentionally put in the root namespace so that anyone using the Bot class will have these
namespace MargieBot
{
    public static class BotExtensions
    {
        public static IResponseProcessor CreateResponseProcessor(this Bot bot, Func<ResponseContext, bool> canRespond, Func<ResponseContext, string> getResponse)
        {
            return new SimpleResponseProcessor() { CanRespondFunction = canRespond, GetResponseFunctions = new List<Func<ResponseContext, BotMessage>>() { (ResponseContext context) => { return new BotMessage() { Text = getResponse(context) }; } } };
        }

        public static MargieSimpleResponseChainer RespondsTo(this Bot bot, string phrase, bool isRegex = false)
        {
            MargieSimpleResponseChainer chainer = new MargieSimpleResponseChainer();
            chainer.ResponseProcessor = new SimpleResponseProcessor();
            if (isRegex) {
                chainer.ResponseProcessor.CanRespondFunction = (ResponseContext context) => {
                    return Regex.IsMatch(context.Message.Text, phrase);
                };
            }
            else {
                chainer.ResponseProcessor.CanRespondFunction = (ResponseContext context) => {
                    return Regex.IsMatch(context.Message.Text, @"\b" + Regex.Escape(phrase) + @"\b", RegexOptions.IgnoreCase);
                };
            }
            bot.ResponseProcessors.Add(chainer.ResponseProcessor);

            return chainer;
        }

        public class MargieSimpleResponseChainer
        {
            internal MargieSimpleResponseChainer() { }
            internal SimpleResponseProcessor ResponseProcessor { get; set; }

            public SimpleResponseProcessor GetResponseProcessor()
            {
                return ResponseProcessor;
            }

            public MargieSimpleResponseChainer With(string response)
            {
                this.ResponseProcessor.GetResponseFunctions.Add((ResponseContext context) => { return new BotMessage() { Text = response }; });
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