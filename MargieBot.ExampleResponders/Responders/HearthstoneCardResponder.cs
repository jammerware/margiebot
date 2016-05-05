using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MargieBot.Models;
using MargieBot.Responders;
using NKeeper;
using NKeeper.Models;

namespace MargieBot.ExampleResponders.Responders
{
    public class HearthstoneCardResponder : IResponder, IDescribable
    {
        // i feel weird about this regex, but whatever for now
        // matches things like [h:annoy-o-tron], [hg:sunwalker], but not [h:]. Manually permitting comma, period, apostrophe, and dash. Kill me now.
        private const string CANRESPOND_REGEX = @"\[h(?<goldRequested>g)?:(?<cardName>[\sa-zA-Z0-9,'\.\-]{2,})]";
        private NKeeperClient _client;

        #region Async "constructor"
        private HearthstoneCardResponder() { }

        public static async Task<HearthstoneCardResponder> CreateAsync()
        {
            var responder = new HearthstoneCardResponder()
            {
                _client = await NKeeperClient.CreateAsync()
            };
            
            return responder;
        }
        #endregion

        #region IDescribable
        public string Description
        {
            get { return "look up Hearthstone cards for ya"; }
        }
        #endregion

        public bool CanRespond(ResponseContext context)
        {
            return Regex.IsMatch(context.Message.Text, CANRESPOND_REGEX);
        }

        public BotMessage GetResponse(ResponseContext context)
        {
            BotMessage response = new BotMessage();
            MatchCollection matches = Regex.Matches(context.Message.Text, CANRESPOND_REGEX);
            int foundCount = 0;
            var attachments = new List<SlackAttachment>();
            List<string> whiffedTerms = new List<string>();

            foreach (Match match in matches)
            {
                string searchTerm = match.Groups["cardName"].Value.Trim();
                bool isGoldRequested = (match.Groups["goldRequested"]?.Value == "g");

                // TODO: change this if i ever support non-english hearthstone translations
                var culture = CultureInfo.CreateSpecificCulture("en-US");

                Card card = _client
                    .Cards
                    .Where(c => culture.CompareInfo.IndexOf(c.Name, searchTerm, CompareOptions.IgnoreCase) >= 0) // case-insensitive "contains"
                    .OrderBy(c => c.Name.Equals(searchTerm, StringComparison.InvariantCultureIgnoreCase) ? 0 : 1)
                    .ThenBy(c => c.Name.StartsWith(searchTerm, StringComparison.InvariantCultureIgnoreCase) ? 0 : 1)
                    .ThenBy(c => c.Name)
                    .FirstOrDefault();

                var text = $"{card.PlayerClass} *{card.Type}* - {{{card.Cost}}} mana";

                if(card.Type == "MINION")
                {
                    text += $" {card.Attack}/{card.Health}";
                }
                else if(card.Type == "WEAPON")
                {
                    text += $" {card.Attack}/{card.Durability}";
                }

                text += $"\n_{card.Flavor}_";

                if (card != null)
                {
                    foundCount = foundCount++;
                    
                    attachments.Add(new SlackAttachment()
                    {
                        ColorHex = "#8C8C8C",
                        Fallback = card.Name,
                        ImageUrl = _client.GetCardImageUrl(card, isGoldRequested),
                        Title = card.Name,
                        TitleLink = _client.GetCardImageUrl(card, isGoldRequested),
                        Text = text,
                        TextFormattingEnabled = true
                    });
                }
                else
                {
                    whiffedTerms.Add(searchTerm);
                }
            }

            response.Attachments = attachments;

            if (whiffedTerms.Count > 0)
            {
                string text = null;

                if (whiffedTerms.Count == 1)
                {
                    text = $@"I couldn't find ""{whiffedTerms[0]}"". Is that somethin' you found on /r/customhearthstone?";
                }
                else if (whiffedTerms.Count == 2)
                {
                    text = @"I couldn't find """ + whiffedTerms[0] + @""" or """ + whiffedTerms[1] + @""". You spellin' those right? Maybe you should watch more Kibler.";
                }
                else
                {
                    text = @"I couldn't find like half of what you just said. You been drinkin' with Harth again? He's a fox, that one.";
                }

                response.Text = text;
            }

            return response;
        }
    }
}