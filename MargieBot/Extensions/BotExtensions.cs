using System.Text.RegularExpressions;
using MargieBot.MessageProcessors;
using MargieBot.Models;

namespace MargieBot.Extensions
{
    public static class BotExtensions
    {
        public delegate void BotMentionedResponseProcessorRequestHandler(SimpleResponseProcessor oldProcessor);

        public static MargieSimpleResponseChainer RespondsTo(this Bot bot, string phrase)
        {
            MargieSimpleResponseChainer chainer = new MargieSimpleResponseChainer();
            chainer.ResponseProcessor = new SimpleResponseProcessor();
            chainer.ResponseProcessor.CanRespondFunction = (MargieContext context) => { return Regex.IsMatch(context.Message.Text, "^" + Regex.Escape(phrase) + "$", RegexOptions.IgnoreCase); };
            bot.ResponseProcessors.Add(chainer.ResponseProcessor);

            // this is here to allow the .IfBotIsMentioned method on the chainer - if they call it, we have to swap the processor to a SimpleBotMentionResponseProcessor and copy the properties to
            // the new processor
            chainer.BotMentionedResponseProcessorRequested += (SimpleResponseProcessor oldProcessor) => {
                chainer.ResponseProcessor = new SimpleBotMentionedResponseProcessor() {
                    CanRespondFunction = oldProcessor.CanRespondFunction,
                    GetResponseFunctions = oldProcessor.GetResponseFunctions
                };

                bot.ResponseProcessors.Remove(oldProcessor);
                bot.ResponseProcessors.Add(chainer.ResponseProcessor);
            };

            return chainer;
        }

        public class MargieSimpleResponseChainer
        {
            public event BotMentionedResponseProcessorRequestHandler BotMentionedResponseProcessorRequested;

            internal MargieSimpleResponseChainer() { }
            public SimpleResponseProcessor ResponseProcessor { get; set; }

            public MargieSimpleResponseChainer With(string response)
            {
                this.ResponseProcessor.GetResponseFunctions.Add((MargieContext context) => { return response; });
                return this;
            }

            public MargieSimpleResponseChainer IfBotIsMentioned()
            {
                if (BotMentionedResponseProcessorRequested != null) {
                    BotMentionedResponseProcessorRequested(ResponseProcessor);
                }

                return this;
            }
        }
    }
}