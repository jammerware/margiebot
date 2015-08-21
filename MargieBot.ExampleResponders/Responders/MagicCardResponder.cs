using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MargieBot.Models;
using MargieBot.Responders;
using Melek.Client.DataStore;
using Melek.Domain;

namespace MargieBot.ExampleResponders.Responders
{
    public sealed class MagicCardResponder : IResponder, IDescribable
    {
        private const string REQUEST_REGEX = @"show (us|me) (?<cardName>[\S\s,]+)";
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
            return (context.Message.ChatHub.Type == SlackChatHubType.DM || context.Message.MentionsBot) && Regex.IsMatch(context.Message.Text, REQUEST_REGEX);
        }

        public BotMessage GetResponse(ResponseContext context)
        {
            string searchTerm = Regex.Match(context.Message.Text, REQUEST_REGEX).Groups["cardName"].Value;
            IReadOnlyList<ICard> results = _MelekClient.Search(searchTerm);

            if(results.Count == 0) {
                return new BotMessage() {
                    Text = @"I couldn't find """ + searchTerm + @""". You spellin' that right? Thought you Magic fellas were good at spells. ;)"
                };
            }
            else {
                IPrinting printing = results.First().GetLastPrinting();
                string message = (results.Count == 1 ? string.Empty : "Not sure exactly... is this what you meant?: ");
                Uri uri = _MelekClient.GetCardImageUri(printing).GetAwaiter().GetResult();
                
                return new BotMessage() {
                    Text = message + uri.AbsoluteUri
                };
            }
        }

        public string Description
        {
            get { return "look up Magic cards for ya"; }
        }
    }
}