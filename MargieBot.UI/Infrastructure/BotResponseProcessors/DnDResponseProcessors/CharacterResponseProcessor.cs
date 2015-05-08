using System.Text.RegularExpressions;
using MargieBot.MessageProcessors;
using MargieBot.Models;

namespace MargieBot.UI.Infrastructure.BotResponseProcessors.DnDResponseProcessors
{
    public class CharacterResponseProcessor : IResponseProcessor
    {
        public bool CanRespond(ResponseContext context)
        {
            return context.Message.MentionsBot && Regex.IsMatch(context.Message.Text, @"\byour (race|class|character|level)\b");
        }

        public string GetResponse(ResponseContext context)
        {
            return string.Empty;
        }
    }
}