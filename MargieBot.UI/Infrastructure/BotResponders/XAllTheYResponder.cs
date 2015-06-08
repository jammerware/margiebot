using System;
using System.Text.RegularExpressions;
using MargieBot.Models;
using MargieBot.Responders;

namespace MargieBot.UI.Infrastructure.BotResponders
{
    public class XAllTheYResponder : IResponder
    {
        private const string XY_REGEX = @"\b(?<x>[\w-]+)\b all the \b(?<y>\w+)\b";

        public bool CanRespond(ResponseContext context)
        {
            return (context.Message.ChatHub.Type == SlackChatHubType.DM || context.Message.MentionsBot) && Regex.IsMatch(context.Message.Text, XY_REGEX);
        }

        public BotMessage GetResponse(ResponseContext context)
        {
            Match match = Regex.Match(context.Message.Text, XY_REGEX);
            return new BotMessage() {
                Text = string.Format("http://apimeme.com/meme?meme=X+All+The+Y&top={0}&bottom=All+the+{1}", match.Groups["x"].Value, match.Groups["y"].Value)
            };
        }
    }
}