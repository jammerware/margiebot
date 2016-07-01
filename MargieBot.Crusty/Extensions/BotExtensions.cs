using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using MargieBot.Models;
using MargieBot.Responders;

// intentionally put in the root namespace so that anyone using the Bot class will have these
namespace MargieBot
{
    public static class BotExtensions
    {
        public static IResponder CreateResponder(this Bot bot, Func<ResponseContext, bool> canRespond, Func<ResponseContext, string> getResponse)
        {
            return new SimpleResponder() { CanRespondFunction = canRespond, GetResponseFunctions = new List<Func<ResponseContext, BotMessage>>() { (ResponseContext context) => { return new BotMessage() { Text = getResponse(context) }; } } };
        }

        public static MargieSimpleResponseChainer RespondsTo(this Bot bot, string phrase, bool isRegex = false)
        {
            MargieSimpleResponseChainer chainer = new MargieSimpleResponseChainer();
            chainer.Responder = new SimpleResponder();
            if (isRegex) {
                chainer.Responder.CanRespondFunction = (ResponseContext context) => {
                    return Regex.IsMatch(context.Message.Text, phrase);
                };
            }
            else {
                chainer.Responder.CanRespondFunction = (ResponseContext context) => {
                    return Regex.IsMatch(context.Message.Text, @"\b" + Regex.Escape(phrase) + @"\b", RegexOptions.IgnoreCase);
                };
            }
            bot.Responders.Add(chainer.Responder);

            return chainer;
        }

        public class MargieSimpleResponseChainer
        {
            internal MargieSimpleResponseChainer() { }
            internal SimpleResponder Responder { get; set; }

            public SimpleResponder GetResponder()
            {
                return Responder;
            }

            public MargieSimpleResponseChainer With(string response)
            {
                this.Responder.GetResponseFunctions.Add((ResponseContext context) => { return new BotMessage() { Text = response }; });
                return this;
            }

            public MargieSimpleResponseChainer IfBotIsMentioned()
            {
                Func<ResponseContext, bool> oldResponseCheck = this.Responder.CanRespondFunction;
                this.Responder.CanRespondFunction = (ResponseContext context) => { return oldResponseCheck(context) && context.Message.MentionsBot; };

                return this;
            }
        }
    }
}