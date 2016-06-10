using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DotNetRunner;
using DotNetRunner.Models;
using MargieBot.Models;
using MargieBot.Responders;

namespace MargieBot.ExampleResponders.Responders
{
    public class NetRunnerCardResponder : IResponder, IDescribable
    {
        // i feel weird about this regex, but whatever for now
        // matches things like [h:annoy-o-tron], [hg:sunwalker], but not [h:]. Manually permitting comma, period, apostrophe, and dash. Kill me now.
        private const string CANRESPOND_REGEX = @"\[n:(?<cardName>[\sa-zA-Z0-9,'\.\-]{2,})]";
        private DnrClient _client;

        #region Async "constructor"
        private NetRunnerCardResponder() { }

        public static async Task<NetRunnerCardResponder> CreateAsync()
        {
            var responder = new NetRunnerCardResponder()
            {
                _client = await DnrClient.CreateAsync()
            };

            return responder;
        }
        #endregion

        #region IDescribable
        public string Description
        {
            get { return "look up NetRunner cards for ya"; }
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

                // TODO: change this if i ever support non-english translations
                var culture = CultureInfo.CreateSpecificCulture("en-US");

                Card card = _client
                    .Cards
                    .Where(c => culture.CompareInfo.IndexOf(c.Title, searchTerm, CompareOptions.IgnoreCase) >= 0) // case-insensitive "contains"
                    .OrderBy(c => c.Title.Equals(searchTerm, StringComparison.InvariantCultureIgnoreCase) ? 0 : 1)
                    .ThenBy(c => c.Title.StartsWith(searchTerm, StringComparison.InvariantCultureIgnoreCase) ? 0 : 1)
                    .ThenBy(c => c.Title)
                    .FirstOrDefault();

                var text = $"*{card.Type}*";

                if(!string.IsNullOrEmpty(card.SubType))
                {
                    text += $": {card.SubType}";
                }

                if(card.Cost != null)
                {
                    text += $" | {card.Cost.Value}";
                }

                if(!string.IsNullOrEmpty(card.FlavorText))
                {
                    text += $"\n_{card.FlavorText}_";
                }

                if (card != null)
                {
                    foundCount = foundCount++;

                    attachments.Add(new SlackAttachment()
                    {
                        ColorHex = "#8C8C8C",
                        Fallback = card.Title,
                        ImageUrl = card.ImageUrl,
                        Title = card.Title,
                        TitleLink = card.Url,
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
                    text = $@"I couldn't find ""{whiffedTerms[0]}"". I know we're all new to this thing, but are you SURE?";
                }
                else if (whiffedTerms.Count == 2)
                {
                    text = @"I couldn't find """ + whiffedTerms[0] + @""" or """ + whiffedTerms[1] + @""". You spellin' those right?";
                }
                else
                {
                    text = @"I couldn't find like half of what you just said. How much brain damage did you take last game?";
                }

                response.Text = text;
            }

            return response;
        }
    }
}