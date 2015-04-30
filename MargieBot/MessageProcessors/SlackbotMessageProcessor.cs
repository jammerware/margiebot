using MargieBot.Models;
using System;

namespace MargieBot.MessageProcessors
{
    public class SlackbotMessageProcessor : IResponseProcessor
    {
        public bool CanRespond(MargieContext context)
        {
            return (context.Message.User == "USLACKBOT" && new Random().Next(8) <= 2);
        }

        public string GetResponse(MargieContext context)
        {
            return context.Phrasebook.GetSlackbotSalutation();
        }
    }
}