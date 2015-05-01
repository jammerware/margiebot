using Bazam.NoobWebClient;
using MargieBot.EventHandlers;
using MargieBot.MessageProcessors;
using MargieBot.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WebSocketSharp;

namespace MargieBot
{
    public class Bot
    {
        private Phrasebook Phrasebook { get; set; }
        public List<IResponseProcessor> ResponseProcessors { get; private set; }
        public IScoringProcessor ScoringProcessor { get; set; }
        private Scorebook Scorebook { get; set; }
        private string SlackKey { get; set; }
        private string TeamID { get; set; }
        private string UserID { get; set; }
        private Dictionary<string, string> UserNameCache { get; set; }
        private WebSocket WebSocket { get; set; }

        public IReadOnlyList<SlackChatHub> ConnectedChannels
        {
            get { return ConnectedHubs.Values.Where(hub => hub.Type == SlackChatHubType.Channel).ToList(); }
        }

        public IReadOnlyList<SlackChatHub> ConnectedDMs
        {
            get { return ConnectedHubs.Values.Where(hub => hub.Type == SlackChatHubType.DM).ToList(); }
        }

        public IReadOnlyList<SlackChatHub> ConnectedGroups
        {
            get { return ConnectedHubs.Values.Where(hub => hub.Type == SlackChatHubType.Group).ToList(); }
        }
        
        public IReadOnlyDictionary<string, SlackChatHub> ConnectedHubs { get; private set; }

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

        public Bot(string slackKey)
        {
            // store the slack key
            this.SlackKey = slackKey;

            // get the books ready
            Phrasebook = new Phrasebook();
            ResponseProcessors = new List<IResponseProcessor>();
            UserNameCache = new Dictionary<string, string>();
        }

        public async Task Connect()
        {
            // disconnect in case we're already connected like a crazy person
            Disconnect();

            NoobWebClient client = new NoobWebClient();
            string json = await client.GetResponse("https://slack.com/api/rtm.start", RequestType.Post, "token", this.SlackKey);
            JObject jData = JObject.Parse(json);

            TeamID = jData["team"]["id"].Value<string>();
            UserID = jData["self"]["id"].Value<string>();
            string webSocketUrl = jData["url"].Value<string>();

            foreach (JObject userObject in jData["users"]) {
                UserNameCache.Add(userObject["id"].Value<string>(), userObject["name"].Value<string>());
            }
            
            // load the channels, groups, and DMs that margie's in
            Dictionary<string, SlackChatHub> hubs = new Dictionary<string, SlackChatHub>();
            ConnectedHubs = hubs;
            
            // channelz
            if (jData["channels"] != null) {
                foreach (JObject channelData in jData["channels"]) {
                    if (!channelData["is_archived"].Value<bool>() && channelData["is_member"].Value<bool>()) {
                        SlackChatHub channel = new SlackChatHub() {
                            ID = channelData["id"].Value<string>(),
                            Name = "#" + channelData["name"].Value<string>(),
                            Type = SlackChatHubType.Channel
                        };
                        hubs.Add(channel.ID, channel);
                    }
                }
            }

            // groupz
            if (jData["groups"] != null) {
                foreach (JObject groupData in jData["groups"]) {
                    if (!groupData["is_archived"].Value<bool>() && groupData["members"].Values<string>().Contains(UserID)) {
                        SlackChatHub group = new SlackChatHub() {
                            ID = groupData["id"].Value<string>(),
                            Name = groupData["name"].Value<string>(),
                            Type = SlackChatHubType.Group
                        };
                        hubs.Add(group.ID, group);
                    }
                }
            }

            // dmz
            if (jData["ims"] != null) {
                foreach (JObject dmData in jData["ims"]) {
                    string userID = dmData["user"].Value<string>();
                    SlackChatHub dm = new SlackChatHub() {
                        ID = dmData["id"].Value<string>(),
                        Name = "@" + (UserNameCache.ContainsKey(userID) ? UserNameCache[userID] : userID),
                        Type = SlackChatHubType.DM
                    };
                    hubs.Add(dm.ID, dm);
                }
            }

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
           JObject jObject = JObject.Parse(json);
            if (jObject["type"].Value<string>() == "message") {
                string channelID = jObject["channel"].Value<string>();
                SlackChatHub hub = null;

                if(ConnectedHubs.ContainsKey(channelID)) {
                    hub = ConnectedHubs[channelID];
                }
                else {
                    hub = SlackChatHub.FromID(channelID);
                    List<SlackChatHub> hubs = new List<SlackChatHub>();
                    hubs.AddRange(ConnectedHubs.Values);
                    hubs.Add(hub);
                }

                SlackMessage message = new SlackMessage() {
                    ChatHub = hub,
                    RawData = json,
                    // some messages may not have text or a user (like unfurled data from URLs)
                    Text = (jObject["text"] != null ? jObject["text"].Value<string>() : null),
                    User = (jObject["user"] != null ? jObject["user"].Value<string>() : null)
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

                // margie can never score or respond to herself and requires that the message have text
                if (message.User != UserID && message.Text != null) {
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
                        if ((!(processor is IBotMentionedResponseProcessor) || Regex.IsMatch(message.Text, "(margie|margie bot|<@" + UserID + ">)", RegexOptions.IgnoreCase)) && processor.CanRespond(context)) {
                            await Say(processor.GetResponse(context), hub);
                            context.MessageHasBeenRespondedTo = true;
                        }
                    }
                }
            }

            RaiseMessageReceived(json);
        }

        public async Task Say(string text, SlackChatHub hub)
        {
            NoobWebClient client = new NoobWebClient();
            await client.GetResponse(
                "https://slack.com/api/chat.postMessage",
                RequestType.Post,
                "token", this.SlackKey,
                "channel", hub.ID,
                "text", text,
                "as_user", "true"
            );
        }

        #region Syntactic sugar
        public MargieSimpleResponseChainer RespondsTo(string simplePhrase, bool requireBotMention = false)
        {
            return new MargieSimpleResponseChainer() { 
                Bot = this,
                CanRespond = (MargieContext context) => { return Regex.IsMatch(context.Message.Text, "^" + Regex.Escape(simplePhrase) + "$", RegexOptions.IgnoreCase); },
                RequireBotMention = requireBotMention
            };
        }
        #endregion?

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