using System.Collections.Generic;
using System.Text.RegularExpressions;
using MargieBot.ExampleResponders.Responders;
using MargieBot.Models;
using MargieBot.Responders;
using MargieBot.UI.Infrastructure.Models.DnD;

namespace MargieBot.UI.Infrastructure.BotResponders.DnDResponders
{
    public class CharacterResponder : IResponder, IDescribable
    {
        public bool CanRespond(ResponseContext context)
        {
            return (context.Message.MentionsBot || context.Message.ChatHub.Type == SlackChatHubType.DM) && Regex.IsMatch(context.Message.Text, @"\byour (race|class|character|level)\b");
        }

        public BotMessage GetResponse(ResponseContext context)
        {
            Character margiesChar = new Character();
            string messageText = string.Format(
                "Well, it's funny y'all should ask. I _just_ got started playin' a game called \"Dungeons and Draggins.\" I don't really know what it's all about yet, but who doesn't like draggin' stuff around in a dungeon?\n\nRight now I'm playin' a level {0} {1} called {2}. I'm real ferocious, y'all. Want to see my character sheet?",
                margiesChar.Level.ToString(),
                margiesChar.Class,
                margiesChar.Name
            );

            BotMessage retVal = new BotMessage() {
                Attachments = new List<SlackAttachment>() { 
                    new SlackAttachment() {
                        ColorHex = "#AD91C2",
                        Fallback = "Margie's character sheet",
                        Fields = new List<SlackAttachmentField>() {
                            new SlackAttachmentField() { IsShort = true, Title = "Race", Value = margiesChar.Race },
                            new SlackAttachmentField() { IsShort = true, Title = "Class", Value = margiesChar.Class },
                            new SlackAttachmentField() { IsShort = true, Title = "Level", Value = margiesChar.Level.ToString() },
                            new SlackAttachmentField() { IsShort = true, Title = "Alignment", Value = margiesChar.Alignment },
                        },
                        ImageUrl = "https://drive.google.com/file/d/0BwPTjHn2z0umMHV6YlpNbUU3Njg/view?usp=sharing",
                        Title =  margiesChar.Name + " | character sheet",
                        TitleLink = "https://drive.google.com/file/d/0BwPTjHn2z0umTDVwelhvOGlsODA/view?usp=sharing",
                    }
                },
                Text = messageText
            };

            return retVal;
        }

        #region IDescribable
        public string Description
        {
            get { return "tell you about my D&D character"; }
        }
        #endregion
    }
}