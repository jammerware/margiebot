using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Bazam.NoobWebClient;
using MargieBot.Infrastructure.EventHandlers;
using MargieBot.Infrastructure.MessageProcessors;
using MargieBot.Infrastructure.Models;
using Newtonsoft.Json.Linq;
using WebSocketSharp;
using System.IO;
using System.Threading.Tasks;

namespace MargieBot.Infrastructure
{
    public class Margie
    {
        private Phrasebook Phrasebook { get; set; }
        private IList<IResponseProcessor> ResponseProcessors { get; set; }
        private IScoringProcessor ScoringProcessor { get; set; }
        private Scorebook Scorebook { get; set; }
        private string SlackKey { get; set; }
        private string TeamID { get; set; }
        private string UserID { get; set; }
        private Dictionary<string, string> UserNameCache { get; set; }
        private WebSocket WebSocket { get; set; }

        public IReadOnlyList<SlackChatHub> ConnectedChannels { get; private set; }
        public IReadOnlyList<SlackChatHub> ConnectedDMs { get; private set; }
        public IReadOnlyList<SlackChatHub> ConnectedGroups { get; private set; }

        private bool _IsConnected = false;
        public bool IsConnected 
        {
            get { return _IsConnected; }
            set
            {
                if (_IsConnected != value) {
                    _IsConnected = value;
                    RaiseConnectionStatusChanged();
                }
            }
        }

        public Margie(string slackKey)
        {
            // store the slack key
            this.SlackKey = slackKey;

            // get the books ready
            Phrasebook = new Phrasebook();
            UserNameCache = new Dictionary<string, string>();

            // initialize the message processors
            // the debug one needs special setup
            DebugResponseProcessor debugProcessor = new DebugResponseProcessor();
            debugProcessor.OnDebugRequested += (string debugText) => {
                File.WriteAllText(DateTime.Now.Ticks.ToString(), debugText);
            };

            // also the ScoreResponseProcessor is pulling double duty as the ScoringProcessor
            ScoringProcessor = new ScoreResponseProcessor();

            ResponseProcessors = new List<IResponseProcessor>();
            ResponseProcessors.Add(new SlackbotMessageProcessor());
            ResponseProcessors.Add(new WhatDoYouDoResponseProcessor());
            ResponseProcessors.Add(new WhatsNewResponseProcessor());
            ResponseProcessors.Add(new YoureWelcomeResponseProcessor());
            ResponseProcessors.Add((IResponseProcessor)ScoringProcessor);
            ResponseProcessors.Add(new ScoreboardRequestMessageProcessor());
            ResponseProcessors.Add(debugProcessor);
            ResponseProcessors.Add(new DefaultMessageProcessor());
        }

        public async Task Connect()
        {
            // disconnect in case we're already connected like a crazy person
            Disconnect();

            NoobWebClient client = new NoobWebClient();
            string json = await client.GetResponse("https://slack.com/api/rtm.start", "token", this.SlackKey);
            JObject jData = JObject.Parse(json);

            TeamID = jData["team"]["id"].Value<string>();
            UserID = jData["self"]["id"].Value<string>();
            string webSocketUrl = jData["url"].Value<string>();

            foreach (JObject userObject in jData["users"]) {
                UserNameCache.Add(userObject["id"].Value<string>(), userObject["name"].Value<string>());
            }
            
            // load the channels, groups, and DMs that margie's in
            List<SlackChatHub> channels = new List<SlackChatHub>();
            List<SlackChatHub> dms = new List<SlackChatHub>();
            List<SlackChatHub> groups = new List<SlackChatHub>();
            
            // channelz
            if (jData["channels"] != null) {
                foreach (JObject channelData in jData["channels"]) {
                    if (!channelData["is_archived"].Value<bool>() && channelData["is_member"].Value<bool>()) {
                        SlackChatHub channel = new SlackChatHub() {
                            ID = channelData["id"].Value<string>(),
                            Name = "#" + channelData["name"].Value<string>(),
                            Type = SlackChatHubType.Channel
                        };
                        channels.Add(channel);
                    }
                }
            }
            ConnectedChannels = channels;

            // groupz
            if (jData["groups"] != null) {
                foreach (JObject groupData in jData["groups"]) {
                    if (!groupData["is_archived"].Value<bool>() && groupData["members"].Values<string>().Contains(UserID)) {
                        SlackChatHub group = new SlackChatHub() {
                            ID = groupData["id"].Value<string>(),
                            Name = groupData["name"].Value<string>(),
                            Type = SlackChatHubType.Group
                        };
                        groups.Add(group);
                    }
                }
            }
            ConnectedGroups = groups;

            // dmz
            if (jData["ims"] != null) {
                foreach (JObject dmData in jData["ims"]) {
                    string userID = dmData["user"].Value<string>();
                    SlackChatHub dm = new SlackChatHub() {
                        ID = dmData["id"].Value<string>(),
                        Name = "@" + (UserNameCache.ContainsKey(userID) ? UserNameCache[userID] : userID),
                        Type = SlackChatHubType.DM
                    };
                    dms.Add(dm);
                }
            }
            ConnectedDMs = dms;

            // start up scorebook for this team
            Scorebook = new Scorebook(TeamID);

            // set up the websocket and connect
            WebSocket = new WebSocket(webSocketUrl);
            WebSocket.OnClose += (object sender, CloseEventArgs e) => {
                IsConnected = false;
            };
            WebSocket.OnMessage += async (object sender, MessageEventArgs args) => {
                await ListenTo(args.Data);
            };
            WebSocket.OnOpen += (object sender, EventArgs e) => {
                IsConnected = true;
            };
            WebSocket.Connect();
        }

        public void Disconnect()
        {
            if (WebSocket != null && WebSocket.IsAlive) WebSocket.Close();
        }

        private async Task ListenTo(string json)
        {
            RaiseMessageReceived(json);

           JObject jObject = JObject.Parse(json);
            if (jObject["type"].Value<string>() == "message") {
                SlackMessage message = new SlackMessage() {
                    Channel = jObject["channel"].Value<string>(),
                    RawData = json,
                    Text = jObject["text"].Value<string>(),
                    User = jObject["user"].Value<string>()
                };

                MargieContext context = new MargieContext() {
                    MargiesUserID = UserID,
                    Message = message,
                    MessageHasBeenRespondedTo = false,
                    Phrasebook = this.Phrasebook,
                    ScoreContext = new ScoreContext() {
                        Scores = Scorebook.GetScores()
                    },
                    UserNameCache = new ReadOnlyDictionary<string, string>(this.UserNameCache)
                };

                // margie can never score or respond to herself
                if (message.User != UserID) {
                    // score first
                    if (ScoringProcessor.IsScoringMessage(message)) {
                        ScoreResult result = ScoringProcessor.Score(message);
                        if (!Scorebook.HasUserScored(result.UserID)) {
                            context.ScoreContext.NewScoreResult = result;
                        }

                        Scorebook.ScoreUser(result);
                    }

                    // then respond
                    foreach (IResponseProcessor processor in ResponseProcessors) {
                        if (processor.CanRespond(context)) {
                            await Say(processor.GetResponse(context), message.Channel);
                            context.MessageHasBeenRespondedTo = true;
                        }
                    }
                }
            }
        }

        private async Task Say(string text, string channel)
        {
            NoobWebClient client = new NoobWebClient();
            await client.GetResponse(
                "https://slack.com/api/chat.postMessage", 
                "token", this.SlackKey, 
                "channel", channel, 
                "text", text,
                "as_user", "true"
            );
        }

        public async Task Say(string text, SlackChatHub hub)
        {
            await Say(text, hub.ID);
        }

        #region Events
        public event MargieConnectionStatusChangedEventHandler ConnectionStatusChanged;
        private void RaiseConnectionStatusChanged()
        {
            if (ConnectionStatusChanged != null) {
                ConnectionStatusChanged(IsConnected);
            }
        }

        public event MargieMessageReceivedEventHandler MessageReceived;
        private void RaiseMessageReceived(string debugText)
        {
            if (MessageReceived != null) {
                MessageReceived(debugText);
            }
        }
        #endregion
    }
}