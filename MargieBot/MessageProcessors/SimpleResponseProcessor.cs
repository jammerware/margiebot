using System;
using System.Collections.Generic;
using MargieBot.Models;

namespace MargieBot.MessageProcessors
{
    public class SimpleResponseProcessor : IResponseProcessor
    {
        public Func<ResponseContext, bool> CanRespondFunction { get; set; }
        public List<Func<ResponseContext, BotMessage>> GetResponseFunctions { get; set; }

        public SimpleResponseProcessor()
        {
            GetResponseFunctions = new List<Func<ResponseContext, BotMessage>>();
        }

        public bool CanRespond(ResponseContext context)
        {
            return CanRespondFunction(context);
        }

        public BotMessage GetResponse(ResponseContext context)
        {
            if (GetResponseFunctions.Count == 0) {
                throw new InvalidOperationException("Attempted to get a response for \"" + context.Message.Text + "\", but no valid responses have been registered.");
            }

            return GetResponseFunctions[new Random().Next(GetResponseFunctions.Count - 1)](context);
        }
    }
}