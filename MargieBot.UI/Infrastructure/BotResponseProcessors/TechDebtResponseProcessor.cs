using MargieBot.MessageProcessors;
using MargieBot.Models;
using System;
using System.Text.RegularExpressions;

namespace MargieBot.UI.Infrastructure.BotResponseProcessors
{
    public class TechDebtResponseProcessor : IResponseProcessor
    {
        public bool CanRespond(MargieContext context)
        {
            return
                Regex.IsMatch(context.Message.Text, "\btechnical debt\b") &&
                context.UserNameCache[context.Message.User] == "obnoxious coworker" &&
                DateTime.Now.Minute > 30;
        }

        public string GetResponse(MargieContext context)
        {
            return "God, " + context.Message.FormattedUser + ", you're such a scruffy-looking nerf-herder.";
        }
    }
}