using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bazam.Extensions;
using MargieBot.Models;
using MargieBot.Responders;
using Melek;
using Melek.Client.DataStore;
using Melek.Client.Vendors;

namespace MargieBot.ExampleResponders.Responders
{
    public sealed class MagicCardResponder : IResponder, IDescribable
    {
        private const string REQUEST_REGEX = @"\[\[(?<cardName>[^\]]+)\]\]|\[m:(?<cardName>[^\]]+)\]";
        private MelekClient _MelekClient = new MelekClient();

        private MagicCardResponder() { }

        public static async Task<MagicCardResponder> GetAsync()
        {
            MagicCardResponder responder = new MagicCardResponder();
            responder._MelekClient = new MelekClient();
            await responder.Initialize();
            return responder;
        }

        private async Task Initialize()
        {
            _MelekClient = new MelekClient() {
                StoreCardImagesLocally = false,
                UpdateCheckInterval = TimeSpan.FromHours(6)
            };

            await _MelekClient.LoadFromDirectory("melek");
        }

        public bool CanRespond(ResponseContext context)
        {
            return Regex.IsMatch(context.Message.Text, REQUEST_REGEX);
        }

        public BotMessage GetResponse(ResponseContext context)
        {
            BotMessage response = new BotMessage();
            MatchCollection matches = Regex.Matches(context.Message.Text, REQUEST_REGEX);
            List<ICard> foundCards = new List<ICard>();
            List<string> whiffedTerms = new List<string>();

            foreach(Match match in matches) {
                string searchTerm = match.Groups["cardName"].Value.Trim();
                ICard result = _MelekClient.Search(searchTerm).FirstOrDefault();

                if(result != null) {
                    foundCards.Add(result);
                }
                else {
                    whiffedTerms.Add(searchTerm);
                }
            }

            if(foundCards.Count > 0) {
                List<SlackAttachment> attachments = new List<SlackAttachment>();
                GathererClient gathererClient = new GathererClient();

                foreach (ICard card in foundCards) {
                    IPrinting printing = card.GetLastPrinting();    
                    Uri uri = _MelekClient.GetImageUri(printing).GetAwaiter().GetResult();

                    string text = card.AllTypes.Distinct().Concatenate(" ");

                    if (card.AllTribes?.Count > 0) {
                        text += " - " + card.AllTribes.Distinct().Concatenate(" ");
                    }

                    if (card.AllCosts != null) {
                        text += ", " + card.AllCosts.Concatenate(" ");
                    }

                    attachments.Add(new SlackAttachment() {
                        ColorHex = "#8C8C8C",
                        Fallback = card.Name,
                        ImageUrl = uri.AbsoluteUri,
                        Title = card.Name,
                        TitleLink = gathererClient.GetLink(card, printing.Set).GetAwaiter().GetResult(),
                        Text = text
                    });
                }

                response.Attachments = attachments;
            }

            if(whiffedTerms.Count > 0) {
                string text = null;

                if (whiffedTerms.Count == 1) {
                    text = @"I couldn't find """ + whiffedTerms[0] + @""". You spellin' that right? Thought you Magic fellas were good at spells. ;)";
                }
                else if (whiffedTerms.Count == 2) {
                    text = @"I couldn't find """ + whiffedTerms[0] + @""" or """ + whiffedTerms[1] + @""". You spellin' those right? Thought you Magic fellas were good at spells. ;)";
                }
                else {
                    text = @"I couldn't find like half of what you just said. You been sniffin' somethin' in a back alley with Fblthp again?";
                }

                response.Text = text;
            }

            return response;
        }

        public string Description
        {
            get { return "look up Magic cards for ya"; }
        }
    }
}