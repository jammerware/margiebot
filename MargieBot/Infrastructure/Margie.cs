using MargieBot.Infrastructure.Debugging;
using MargieBot.Infrastructure.MessageProcessors;
using MargieBot.Infrastructure.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Bazam.NoobWebClient;

namespace MargieBot.Infrastructure
{
    public class Margie
    {
        private Phrasebook Phrasebook { get; set; }
        private IList<IResponseProcessor> ResponseProcessors { get; set; }
        private Scorebook Scorebook { get; set; }
        public string UserID { get; private set; }

        public Margie()
        {
            // get the books ready
            Phrasebook = new Phrasebook();
            Scorebook = new Scorebook();

            // initialize the message processors
            // the debug one needs special setup
            DebugMessageProcessor debugProcessor = new DebugMessageProcessor();
            debugProcessor.OnDebugRequested += RaiseDebugRequested;

            ResponseProcessors = new List<IResponseProcessor>();
            ResponseProcessors.Add(new SlackbotMessageProcessor());
            ResponseProcessors.Add(new ScoreResponseProcessor());
            ResponseProcessors.Add(debugProcessor);
            ResponseProcessors.Add(new DefaultMessageProcessor());
        }

        public async Task<string> GetSocketUrl()
        {
            // this is iffy. All the talking-to-Slack code is in Margie, and the call that gets the socket URL also retrieves her ID
            // and she needs to remember it. But it feels wrong to have her ID being set as a side effect of this operation.
            // Think about it.
            NoobWebClient client = new NoobWebClient();
            string json = await client.GetResponse("https://slack.com/api/rtm.start", "token", "xoxb-4599190677-HJTfW7q5O4hwaBqMBbEl4RBG");
            JObject jObject = JObject.Parse(json);
            UserID = jObject["self"]["id"].Value<string>();
            return jObject["url"].Value<string>();
        }

        public void ListenTo(string json)
        {
            JObject jObject = JObject.Parse(json);
            if (jObject["type"] != null && jObject["type"].Value<string>() == "message") {
                SlackMessage message = new SlackMessage() {
                    Channel = jObject["channel"].Value<string>(),
                    RawData = json,
                    Text = jObject["text"].Value<string>(),
                    User = jObject["user"].Value<string>()
                };

                MargieContext context = new MargieContext() {
                    MargiesUserID = UserID,
                    MessageHasBeenRespondedTo = false,
                    Phrasebook = this.Phrasebook,
                    UserContext = new UserContext() {
                        HasScoredPreviously = Scorebook.HasUserScored(message.User),
                        Score = Scorebook.GetUserScore(message.User)
                    }
                };

                foreach (IResponseProcessor processor in ResponseProcessors) {
                    if (processor.CanRespond(context)) {
                        Say(processor.GetResponse(context), message.Channel);
                        context.MessageHasBeenRespondedTo = true;
                    }
                }
            }
        }

        private async void Say(string text, string channel)
        {
            NoobWebClient client = new NoobWebClient();
            await client.GetResponse(
                "https://slack.com/api/chat.postMessage", 
                "token", Constants.AUTH_TOKEN, 
                "channel", channel, 
                "text", text,
                "as_user", "true"
            );
        }

        #region Events
        public event MargieDebuggingEventHandler OnDebugRequested;
        private void RaiseDebugRequested(string debugMessage, string completeJson)
        {
            if (OnDebugRequested != null) {
                OnDebugRequested(debugMessage, completeJson);
            }
        }
        #endregion
    }
}