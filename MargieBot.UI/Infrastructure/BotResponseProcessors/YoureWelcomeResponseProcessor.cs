using MargieBot.MessageProcessors;
using MargieBot.Models;
using System.Text.RegularExpressions;

namespace MargieBot.UI.Infrastructure.BotResponseProcessors
{
    public class YoureWelcomeResponseProcessor : IResponseProcessor
    {
        public bool CanRespond(MargieContext context)
        {
            return Regex.IsMatch(context.Message.Text, @"\b(thanks|thank you)\b", RegexOptions.IgnoreCase);
        }

        public string GetResponse(MargieContext context)
        {
            return context.Phrasebook.GetYoureWelcome();
        }

        public bool ResponseRequiresBotMention(MargieContext context)
        {
            return true;
        }
    }
}