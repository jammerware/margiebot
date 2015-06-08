using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Bazam.NoobWebClient;
using MargieBot.Models;
using MargieBot.Responders;

namespace MargieBot.ExampleResponders.Responders
{
    public class DefineResponder : IResponder
    {
        private const string DEFINE_REGEX = @"define\s+(?<term>\w+)";
        private string ApiKey { get; set; }

        public DefineResponder(string apiKey)
        {
            ApiKey = apiKey;
        }

        public bool CanRespond(ResponseContext context)
        {
            return (context.Message.MentionsBot || context.Message.ChatHub.Type == SlackChatHubType.DM) && Regex.IsMatch(context.Message.Text, DEFINE_REGEX);
        }

        public BotMessage GetResponse(ResponseContext context)
        {
            string term = WebUtility.UrlEncode(Regex.Match(context.Message.Text, DEFINE_REGEX).Groups["term"].Value);

            NoobWebClient client = new NoobWebClient();
            string definitionData = client.GetResponse(
                string.Format(
                    "http://www.dictionaryapi.com/api/v1/references/collegiate/xml/{0}?key={1}", 
                    term,
                    ApiKey
                ),
                RequestMethod.Get
            ).GetAwaiter().GetResult();

            XElement root = XElement.Parse(definitionData);

            if (root.Descendants("suggestion").FirstOrDefault() != null) {
                return new BotMessage() {
                    Text = "You know what? I don't even know what that is, and neither does my buddy WebsterBot. And he's super smart, y'all. He wanted to know if maybe you meant *" + root.Descendants("suggestion").First().Value + "*?"
                };
            }
            else if (root.Descendants("ew").FirstOrDefault() == null) {
                return new BotMessage() {
                    Text = "Are y'all funnin' with me again? That can't be a real thing, can it?"
                };
            }
            else {
                string word = root.Descendants("ew").First().Value;
                string partOfSpeech = root.Descendants("fl").First().Value;
                string definition = root.Descendants("dt").First().Value;
                string etymology = null;
                string audioFile = null;

                if (root.Descendants("et").FirstOrDefault() != null) {
                    etymology = root.Descendants("et").First().Value;
                    etymology = Regex.Replace(etymology, "</?it>", "_");
                }

                // compute the sound url thing
                if (root.Descendants("sound") != null) {
                    audioFile = root.Descendants("wav").First().Value;

                    // do a bunch of dumb stuff to find the audio file URL because this API is wacky
                    // http://www.dictionaryapi.com/info/faq-audio-image.htm#collegiate
                    if (audioFile.StartsWith("bix")) {
                        audioFile = "bix/" + audioFile;
                    }
                    else if (audioFile.StartsWith("gg")) {
                        audioFile = "gg/" + audioFile;
                    }
                    else if (audioFile.StartsWith("number")) {
                        audioFile = "number/" + audioFile;
                    }
                    else {
                        audioFile = audioFile.Substring(0, 1) + "/" + audioFile;
                    }

                    audioFile = "http://media.merriam-webster.com/soundc11/" + audioFile;
                }

                string[] defSplits = definition.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                if (defSplits.Length > 1) {
                    StringBuilder defBuilder = new StringBuilder();

                    foreach (string def in defSplits) {
                        defBuilder.Append("• " + def + "\n");
                    }

                    definition = defBuilder.ToString();
                }
                else { definition = definition.Replace(":", string.Empty); }

                return new BotMessage() {
                    Attachments = new List<SlackAttachment>() {
                    new SlackAttachment() {
                        ColorHex = "#AD91C2",
                        Fallback = "Define " + term,
                        Fields = new List<SlackAttachmentField>() {
                            new SlackAttachmentField() { IsShort = true, Title = "Definition", Value = definition },
                            new SlackAttachmentField() { IsShort = true, Title = "Part of Speech", Value = partOfSpeech },
                            new SlackAttachmentField() { IsShort = true, Title = "Etymology", Value = (!string.IsNullOrEmpty(etymology) ? etymology : "It's a made up word. They all are.") },
                            new SlackAttachmentField() { IsShort = true, Title = "How to Say It", Value = audioFile },
                        },
                        Title =  "Define: " + term,
                        TitleLink = "http://dictionary.reference.com/browse/" + WebUtility.UrlEncode(term),
                    }
                },
                    Text = "Well, I have literally _no_ idea what that means, so I called my friend WebsterBot."
                };
            }
        }
    }
}