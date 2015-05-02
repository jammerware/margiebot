using System;
using System.Collections.Generic;
using MargieBot.Models;

namespace MargieBot.MessageProcessors
{
    internal class SimpleResponseProcessor : IResponseProcessor
    {
        public Func<MargieContext, bool> CanRespondFunction { get; set; }
        public List<Func<MargieContext, string>> GetResponseFunctions { get; set; }

        public SimpleResponseProcessor()
        {
            GetResponseFunctions = new List<Func<MargieContext, string>>();
        }

        public bool CanRespond(MargieContext context)
        {
            return CanRespondFunction(context);
        }

        public string GetResponse(MargieContext context)
        {
            if (GetResponseFunctions.Count == 0) {
                throw new InvalidOperationException("Attempted to get a response for \"" + context.Message.Text + "\", but no valid responses have been registered.");
            }
            return GetResponseFunctions[new Random().Next(GetResponseFunctions.Count - 1)](context);
        }
    }
}