using MargieBot.Infrastructure.Debugging;
using MargieBot.Infrastructure.MessageProcessors;
using MargieBot.Infrastructure.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace MargieBot.Infrastructure
{
    public class Margie
    {
        public event MargieDebuggingEventHandler OnDebugRequested;

        private IList<IMessageProcessor> _MessageProcessors;

        private Scorebook _Scorebook;
        public Scorebook Scorebook
        {
            get { return _Scorebook; }
        }

        public string UserID { get; private set; }

        public Margie()
        {
            // get the scorebook ready
            _Scorebook = new Scorebook();

            // initialize the message processors
            // the debug one needs special setup
            DebugMessageProcessor debugProcessor = new DebugMessageProcessor();
            debugProcessor.OnDebugRequested += RaiseDebugRequested;

            _MessageProcessors = new List<IMessageProcessor>();
            _MessageProcessors.Add(new SlackbotMessageProcessor());
            _MessageProcessors.Add(new ScoreMessageProcessor());
            _MessageProcessors.Add(debugProcessor);
            _MessageProcessors.Add(new DefaultMessageProcessor());
        }

        public string GetAffirmation()
        {
            string[] affirmations = new string[] {
                "Git it!",
                "Give it up!",
                "Nice goin'!",
                "Yeah, buddy!",
                "You go!"
            };

            return affirmations[new Random().Next(affirmations.Length)];
        }

        public string GetExclamation()
        {
            string[] exclamations = new string[] {
                "Awwwright, y'all!",
                "Hoo boy!",
                "Whooo!",
                "Y'all!",
                "Yahoo!"
            };

            return exclamations[new Random().Next(exclamations.Length - 1)];
        }

        public string GetQuery()
        {
            string[] queries = new string[] {
                "Hey, hun! What's up?",
                "Hey, sugar. Need help?",
                "Well, hi there, puddin'. What can I do for ya?",
                "*[yawns]*. Whew. 'Scuse me. Sorry 'bout that. You rang?"
            };

            return queries[new Random().Next(queries.Length - 1)];
        }

        public string GetSlackbotSalutation()
        {
            string[] salutations = new string[] {
                "Hey, Slackbot! How you doin', cutie?",
                "Mornin', Slackbot! Heard you were out with Rita Bot last night. How'd it go?",
                "Well, howdy, Slackbot. You're lookin' mighty handsome today."
            };

            return salutations[new Random().Next(salutations.Length - 1)];
        }

        public async Task<string> GetSocketUrl()
        {
            // this is iffy. All the talking-to-Slack code is in Margie, and the call that gets the socket URL also retrieves her ID
            // and she needs to remember it. But it feels weird to have her ID being set as a side effect of this operation.
            // Think about it.
            string json = await GetResponse("https://slack.com/api/rtm.start", "token", "xoxb-4599190677-HJTfW7q5O4hwaBqMBbEl4RBG");
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

                bool hasBeenRespondedTo = false;
                foreach (IMessageProcessor processor in _MessageProcessors) {
                    if (processor.IsRelevant(message, this, hasBeenRespondedTo)) {
                        processor.Respond(message, this);
                        hasBeenRespondedTo = true;
                    }
                }
            }
        }

        public async void Say(string text, string channel)
        {
            await GetResponse(
                "https://slack.com/api/chat.postMessage", 
                "token", Constants.AUTH_TOKEN, 
                "channel", channel, 
                "text", text,
                "as_user", "true"
            );
        }

        private Task<string> GetResponse(string address, params string[] values)
        {
            Dictionary<string, string> dictValues = new Dictionary<string, string>();

            if (values != null && values.Length > 1) {
                string key = string.Empty;
                for (int i = 0; i < values.Length; i++) {
                    if (i % 2 == 0) {
                        key = values[i];
                    }
                    else {
                        dictValues.Add(key, values[i]);
                    }
                }
            }

            return GetResponse(address, dictValues);
        }

        private async Task<string> GetResponse(string address, Dictionary<string, string> bodyValues = null)
        {
            FormUrlEncodedContent content = new FormUrlEncodedContent(bodyValues);
            using(HttpClient client = new HttpClient()) {
                HttpResponseMessage response = await client.PostAsync(address, content);
                return await response.Content.ReadAsStringAsync();

            }
        }

        private void RaiseDebugRequested(string debugMessage, string completeJson)
        {
            if (OnDebugRequested != null) {
                OnDebugRequested(debugMessage, completeJson);
            }
        }
    }
}