using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bazam.Extensions;
using MargieBot.Models;
using MargieBot.Responders;
using Melek.Client.DataStore;
using Melek.Client.Vendors;
using Melek.Domain;

namespace MargieBot.ExampleResponders.Responders
{
    public sealed class MagicCardResponder : IResponder, IDescribable
    {
        private const string REQUEST_REGEX = @"\[\[(?<cardName>[\S\s,]+)\]\]";
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
            string searchTerm = Regex.Match(context.Message.Text, REQUEST_REGEX).Groups["cardName"].Value;
            IReadOnlyList<ICard> results = _MelekClient.Search(searchTerm);

            if (results.Count == 0) {
                return new BotMessage() {
                    Text = @"I couldn't find """ + searchTerm + @""". You spellin' that right? Thought you Magic fellas were good at spells. ;)"
                };
            }
            else {
                ICard card = results.First();
                IPrinting printing = card.GetLastPrinting();
                GathererClient gathererClient = new GathererClient();
                Uri uri = _MelekClient.GetCardImageUri(printing).GetAwaiter().GetResult();

                string text = card.AllTypes.Concatenate(" ");
                if(card.AllTribes != null) {
                    text += " - " + card.AllTribes.Concatenate(" ");
                }
                if(card.AllCosts != null) {
                    text += ", " + card.AllCosts.Concatenate(" ");
                }

                return new BotMessage() {
                    Attachments = new SlackAttachment[] {
                        new SlackAttachment() {
                            ColorHex = "#8C8C8C",
                            Fallback = card.Name,
                            ImageUrl = uri.AbsoluteUri,
                            Title = card.Name,
                            TitleLink = gathererClient.GetLink(card, printing.Set).GetAwaiter().GetResult(),
                            Text = text
                        }
                    }
                };
            }
        }

        public string Description
        {
            get { return "look up Magic cards for ya"; }
        }
    }
}