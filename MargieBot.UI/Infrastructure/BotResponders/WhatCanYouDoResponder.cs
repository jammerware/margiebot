using System.Text.RegularExpressions;
using MargieBot.Models;
using MargieBot.Responders;
using MargieBot.UI.Infrastructure.Models;

namespace MargieBot.UI.Infrastructure.BotResponders
{
    public class WhatCanYouDoResponder : IResponder
    {
        public bool CanRespond(ResponseContext context)
        {
            return (context.Message.MentionsBot || context.Message.ChatHub.Type == SlackChatHubType.DM) && Regex.IsMatch(context.Message.Text, "what (can|do) you do");
        }

        public BotMessage GetResponse(ResponseContext context)
        {
            return new BotMessage() {
                Text = "I do lotsa things! Here are some of the more fun ones:\n\n```- " + string.Join("\n- ", context.Get<ResponderSummary>().Summaries) + "```"
            };
        }
    }
}