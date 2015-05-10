using System.Collections.Generic;
using System.Text.RegularExpressions;
using MargieBot.MessageProcessors;
using MargieBot.Models;
using MargieBot.UI.Infrastructure.Models.DnD;

namespace MargieBot.UI.Infrastructure.BotResponseProcessors.DnDResponseProcessors
{
    public class CharacterResponseProcessor : IResponseProcessor
    {
        public bool CanRespond(ResponseContext context)
        {
            return context.Message.MentionsBot && Regex.IsMatch(context.Message.Text, @"\byour (race|class|character|level)\b");
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
                        ImageUrl = "https://slack-files.com/files-pub/T02FW532C-F04PL4DK6-4326a9bd7c/margie-warlock.jpg",
                        Title =  margiesChar.Name + " | character sheet",
                        TitleLink = "https://drive.google.com/file/d/0BwPTjHn2z0umSm5YS1NadXFVYXc/view?usp=sharing",
                    }
                },
                Text = messageText
            };

            return retVal;
        }
    }
}