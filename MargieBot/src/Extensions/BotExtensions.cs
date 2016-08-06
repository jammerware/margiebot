using System.Collections.Generic;
using System.Text.RegularExpressions;
using MargieBot.Models;
using MargieBot.Responders;

// intentionally put in the root namespace so that anyone using the Bot class will have these
namespace MargieBot
{
    public static class BotExtensions
    {
        public static MargieSimpleResponseChainer RespondsTo(this Bot bot, string phrase, bool isRegex = false)
        {
            MargieSimpleResponseChainer chainer = new MargieSimpleResponseChainer();
            chainer.Responder = new SimpleResponder();

            if (isRegex)
            {
                chainer.Responder.CanRespondFunction = (ResponseContext context) =>
                {
                    return Regex.IsMatch(context.Message.Text, phrase);
                };
            }
            else
            {
                chainer.Responder.CanRespondFunction = (ResponseContext context) =>
                {
                    return Regex.IsMatch(context.Message.Text, @"\b" + Regex.Escape(phrase) + @"\b", RegexOptions.IgnoreCase);
                };
            }

            // this seems wrong - probably not thread safe?
            var responders = new List<IResponder>(bot.Responders);
            responders.Add(chainer.Responder);
            bot.Responders = responders;

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
                Responder.GetResponseFunctions.Add((ResponseContext context) => { return new BotMessage() { Text = response }; });
                return this;
            }

            public MargieSimpleResponseChainer IfBotIsMentioned()
            {
                var oldResponseCheck = Responder.CanRespondFunction;
                Responder.CanRespondFunction = (ResponseContext context) => { return oldResponseCheck(context) && context.Message.MentionsBot; };

                return this;
            }
        }
    }
}