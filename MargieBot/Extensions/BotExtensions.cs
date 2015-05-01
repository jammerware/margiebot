using MargieBot.MessageProcessors;
using MargieBot.Models;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MargieBot.Extensions
{
    public static class BotExtensions
    {
        public static MargieSimpleResponseChainer RespondsTo(this Bot bot, string phrase)
        {
            MargieSimpleResponseChainer chainer = new MargieSimpleResponseChainer();
            chainer.ResponseProcessor = new SimpleResponseProcessor();
            chainer.ResponseProcessor.CanRespondFunction = (MargieContext context) => { return Regex.IsMatch(context.Message.Text, "^" + Regex.Escape(phrase) + "$", RegexOptions.IgnoreCase); };
            bot.ResponseProcessors.Add(chainer.ResponseProcessor);
            return chainer;
        }

        public class SimpleResponseProcessor : IResponseProcessor
        {
            public Func<MargieContext, bool> CanRespondFunction { get; set; }
            public List<Func<MargieContext, string>> GetResponseFunctions { get; set; }

            internal SimpleResponseProcessor()
            {
                GetResponseFunctions = new List<Func<MargieContext, string>>();
            }

            public bool CanRespond(MargieContext context)
            {
                return CanRespondFunction(context);
            }

            public string GetResponse(MargieContext context)
            {
                if(GetResponseFunctions.Count == 0) {
                    throw new InvalidOperationException("Attempted to get a response for \"" + context.Message.Text + "\", but no valid responses have been registered.");
                }
                return GetResponseFunctions[new Random().Next(GetResponseFunctions.Count - 1)](context);
            }
        }

        public class MargieSimpleResponseChainer
        {
            internal MargieSimpleResponseChainer() { }
            public SimpleResponseProcessor ResponseProcessor { get; set; }

            public MargieSimpleResponseChainer With(string response)
            {
                this.ResponseProcessor.GetResponseFunctions.Add((MargieContext context) => { return response; });
                return this;
            }
        }
    }
}