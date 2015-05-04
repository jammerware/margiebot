using System;
using System.Collections.Generic;
using MargieBot.Models;

namespace MargieBot.MessageProcessors
{
    internal class SimpleResponseProcessor : IResponseProcessor
    {
        public Func<ResponseContext, bool> CanRespondFunction { get; set; }
        public List<Func<ResponseContext, string>> GetResponseFunctions { get; set; }

        public SimpleResponseProcessor()
        {
            GetResponseFunctions = new List<Func<ResponseContext, string>>();
        }

        public bool CanRespond(ResponseContext context)
        {
            return CanRespondFunction(context);
        }

        public string GetResponse(ResponseContext context)
        {
            if (GetResponseFunctions.Count == 0) {
                throw new InvalidOperationException("Attempted to get a response for \"" + context.Message.Text + "\", but no valid responses have been registered.");
            }
            return GetResponseFunctions[new Random().Next(GetResponseFunctions.Count - 1)](context);
        }
    }
}