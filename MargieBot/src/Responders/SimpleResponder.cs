using System;
using System.Collections.Generic;

namespace MargieBot
{
    public class SimpleResponder : IResponder
    {
        public Func<ResponseContext, bool> CanRespondFunction { get; set; }
        public List<Func<ResponseContext, BotMessage>> GetResponseFunctions { get; set; }

        public SimpleResponder()
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

        #region Utility
        public static SimpleResponder Create(Func<ResponseContext, bool> canRespond, Func<ResponseContext, string> getResponse)
        {
            return new SimpleResponder()
            {
                CanRespondFunction = canRespond,
                GetResponseFunctions = new List<Func<ResponseContext, BotMessage>>()
                {
                    (ResponseContext context) =>
                    {
                        return new BotMessage() { Text = getResponse(context) };
                    }
                }
            };
        }
        #endregion
    }
}