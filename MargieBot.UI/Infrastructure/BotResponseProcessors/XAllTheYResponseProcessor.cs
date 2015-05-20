using System;
using System.Text.RegularExpressions;
using MargieBot.MessageProcessors;
using MargieBot.Models;

namespace MargieBot.UI.Infrastructure.BotResponseProcessors
{
    public class XAllTheYResponseProcessor : IResponseProcessor
    {
        private const string XY_REGEX = @"\b(?<x>\w+)\b all the \b(?<y>\w+)\b";

        public bool CanRespond(ResponseContext context)
        {
            return Regex.IsMatch(context.Message.Text, XY_REGEX);
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