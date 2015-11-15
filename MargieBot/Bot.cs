using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bazam.Http;
using MargieBot.EventHandlers;
using MargieBot.Models;
using MargieBot.Responders;
using MargieBot.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebSocketSharp;

namespace MargieBot
{
    public class Bot
    {
        #region Private properties
        private string _BotNameRegex;
        private string BotNameRegex
        {
            get 
            {
                // only build the regex if we're connected - if we're not connected we won't know our bot's name or user ID
                if (_BotNameRegex == string.Empty && IsConnected) {
                    _BotNameRegex = new BotNameRegexComposer().ComposeFor(UserName, UserID, Aliases);
                }

                return _BotNameRegex;
            }
            set { _BotNameRegex = value; }
        }
        
        private Dictionary<string, string> UserNameCache { get; set; }
        private WebSocket WebSocket { get; set; }
        #endregion

        #region Public properties
        private IReadOnlyList<string> _Aliases;
        public IReadOnlyList<string> Aliases
        {
            get { return _Aliases; }
            set
            {
                _Aliases = value;
                BotNameRegex = string.Empty;
            }
        }
        public List<IResponder> Responders { get; private set; }
        
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

        public bool IsConnected 
        {
            get { return ConnectedSince != null; }
        }

        private DateTime? _ConnectedSince = null;
        public DateTime? ConnectedSince
        {
            get { return _ConnectedSince; }
            set
            {
                if (_ConnectedSince != value) {
                    _ConnectedSince = value;
                    RaiseConnectionStatusChanged();
                }
            }
        }

        public Dictionary<string, object> ResponseContext { get; private set; }
        public string SlackKey { get; private set; }
        public string TeamID { get; private set; }
        public string TeamName { get; private set; }
        public string UserID { get; private set; }
        public string UserName { get; private set; }
        #endregion

        public Bot()
        {
            // get the books ready
            Aliases = new List<string>();
            ResponseContext = new Dictionary<string, object>();
            Responders = new List<IResponder>();
            UserNameCache = new Dictionary<string, string>();
        }

        public async Task Connect(string slackKey)
        {
            this.SlackKey = slackKey;

            // disconnect in case we're already connected like a crazy person
            Disconnect();

            // kill the regex for our bot's name - we'll rebuild it upon request with some of the info we get here
            BotNameRegex = string.Empty;

            NoobWebClient client = new NoobWebClient();
            string json = await client.DownloadString("https://slack.com/api/rtm.start", RequestMethod.Post, "token", this.SlackKey);
            JObject jData = JObject.Parse(json);

            TeamID = jData["team"]["id"].Value<string>();
            TeamName = jData["team"]["name"].Value<string>();
            UserID = jData["self"]["id"].Value<string>();
            UserName = jData["self"]["name"].Value<string>();
            string webSocketUrl = jData["url"].Value<string>();

            UserNameCache.Clear();
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

            // set up the websocket and connect
            WebSocket = new WebSocket(webSocketUrl);
            WebSocket.OnOpen += (object sender, EventArgs e) => {
                // set connection-related properties
                ConnectedSince = DateTime.Now;
            };
            WebSocket.OnMessage += async (object sender, MessageEventArgs args) => {
                await ListenTo(args.Data);
            };
            WebSocket.OnClose += (object sender, CloseEventArgs e) => {
                // set connection-related properties
                ConnectedSince = null;
                TeamID = null;
                TeamName = null;
                UserID = null;
                UserName = null;
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

                if (ConnectedHubs.ContainsKey(channelID))
                {
                    hub = ConnectedHubs[channelID];
                }
                else
                {
                    hub = SlackChatHub.FromID(channelID);
                    Dictionary<string, SlackChatHub> hubs = new Dictionary<string, SlackChatHub>(ConnectedHubs.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
                    hubs.Add(hub.ID, hub);
                    ConnectedHubs = hubs;
                }

                string messageText = jObject["text"] != null ? jObject["text"].Value<string>() : null;

                string subType = jObject["subtype"] != null ? jObject["subtype"].Value<string>() : null;
                bool hidden = jObject["hidden"] != null ? jObject["hidden"].Value<bool>() : false;
                string timestamp = jObject["ts"] != null ? jObject["ts"].Value<string>() : null;

                string userId = jObject["user"] != null ? jObject["user"].Value<string>() : null;

                string subMessage = null;
                if (string.IsNullOrEmpty(messageText) && jObject["message"] != null)
                {
                    JObject subJObject = jObject["message"].Value<JObject>();
                    messageText = (subJObject["text"] != null ? subJObject["text"].Value<string>() : null);
                    userId = subJObject["user"] != null ? subJObject["user"].Value<string>() : null;
                    timestamp = subJObject["ts"] != null ? subJObject["ts"].Value<string>() : null;
                }

                // check to see if bot has been mentioned
                SlackMessage message = new SlackMessage() {
                    ChatHub = hub,
                    MentionsBot = (messageText != null ? Regex.IsMatch(messageText, BotNameRegex, RegexOptions.IgnoreCase) : false),
                    RawData = json,
                    // some messages may not have text or a user (like unfurled data from URLs)
                    Text = messageText,
                    SubType = subType,
                    Hidden = hidden,
                    Timestamp = timestamp,
                    User = !string.IsNullOrEmpty(userId) ? new SlackUser() { ID = userId } : null
                };

                ResponseContext context = new ResponseContext() {
                    BotHasResponded = false,
                    BotUserID = UserID,
                    BotUserName = UserName,
                    Message = message,
                    TeamID = this.TeamID,
                    UserNameCache = new ReadOnlyDictionary<string, string>(this.UserNameCache)
                };

                // if the end dev has added any static entries to the ResponseContext collection of Bot, add them to the context being passed to the responders.
                if (ResponseContext != null) {
                    foreach (string key in ResponseContext.Keys) {
                        context.Set(key, ResponseContext[key]);
                    }
                }

                // margie can never respond to herself and requires that the message have text and be from an actual person
                if (message.User != null && message.User.ID != UserID && message.Text != null) {
                    foreach (IResponder responder in Responders) {
                        if (responder.CanRespond(context)) {
                            if (responder.GetResponseType() == BotResponseType.Message)
                            {
                                await Say((BotMessage)responder.GetResponse(context), context);
                                context.BotHasResponded = true;
                            }
                            else if (responder.GetResponseType() == BotResponseType.Reaction)
                            {
                                await React((BotReaction)responder.GetResponse(context), context);
                                context.BotHasResponded = true;
                            }
                        }
                    }
                }
            }

            RaiseMessageReceived(json);
        }

        public async Task Say(BotMessage message)
        {
            await Say(message, null);
        }

        private async Task Say(BotMessage message, ResponseContext context)
        {
            string chatHubID = null;
            if (message == null)
            {
                return;
            }

            if (message.ChatHub != null) {
                chatHubID = message.ChatHub.ID;
            }
            else if(context != null && context.Message.ChatHub != null) {
                chatHubID = context.Message.ChatHub.ID;
            }

            if(chatHubID != null) {
                NoobWebClient client = new NoobWebClient();

                List<string> values = new List<string>() {
                    "token", this.SlackKey,
                    "channel", chatHubID,
                    "text", message.Text,
                    "as_user", "true"
                };

                if (message.Attachments.Count > 0) {
                    values.Add("attachments");
                    values.Add(JsonConvert.SerializeObject(message.Attachments));
                }

                await client.DownloadString(
                    "https://slack.com/api/chat.postMessage",
                    RequestMethod.Post,
                    values.ToArray()
                );
            }
            else {
                throw new ArgumentException("When calling the Say() method, the message parameter must have its ChatHub property set.");
            }
        }

        private async Task React(BotReaction reaction, ResponseContext context)
        {
            NoobWebClient client = new NoobWebClient();

            List<string> values = new List<string>() {
                "token", this.SlackKey,
                "name", reaction.Name
            };

            switch (reaction.ReactionType)
            {
                case BotReactionType.ChannelMessage:
                    values.Add("timestamp");
                    values.Add(reaction.TimeStamp);
                    values.Add("channel");
                    break;
                case BotReactionType.File:
                    values.Add("file");
                    break;
                case BotReactionType.FileComment:
                    values.Add("file_comment");
                    break;
            }

            // Add source ID for any of the 3 types
            values.Add(reaction.SourceId);

            var result = await client.DownloadString(
                "https://slack.com/api/reactions.add",
                RequestMethod.Post,
                values.ToArray()
            );
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