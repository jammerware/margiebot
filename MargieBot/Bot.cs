using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bazam.NoobWebClient;
using MargieBot.EventHandlers;
using MargieBot.MessageProcessors;
using MargieBot.Models;
using Newtonsoft.Json.Linq;
using WebSocketSharp;

namespace MargieBot
{
    public class Bot
    {
        #region Private properties
        private string _BotNameRegex;
        public string BotNameRegex
        {
            get 
            {
                // only build the regex if we're connected - if we're not connected we won't know our bot's name or user ID
                if (_BotNameRegex == string.Empty && IsConnected) {
                    StringBuilder builder = new StringBuilder();
                    builder.Append(@"(<@" + UserID + @">|");
                    builder.Append(@"\b" + UserName + @"\b");

                    foreach (string pseudonym in Aliases) {
                        builder.Append(@"|\b" + pseudonym + @"\b");
                    }
                    builder.Append(@")");
                    _BotNameRegex = builder.ToString();
                }

                return _BotNameRegex;
            }
            set { _BotNameRegex = value; }
        }
        
        private string SlackKey { get; set; }
        private string TeamID { get; set; }
        private string UserID { get; set; }
        private string UserName { get; set; }
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
        public List<IResponseProcessor> ResponseProcessors { get; private set; }
        
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

        public Dictionary<string, object> StaticResponseContextData { get; set; }
        #endregion

        public Bot(string slackKey)
        {
            // store the slack key
            this.SlackKey = slackKey;

            // get the books ready
            Aliases = new List<string>();
            ResponseProcessors = new List<IResponseProcessor>();
            UserNameCache = new Dictionary<string, string>();
        }

        public async Task Connect()
        {
            // disconnect in case we're already connected like a crazy person
            Disconnect();

            // kill the regex for our bot's name - we'll rebuild it upon request with some of the info we get here
            BotNameRegex = string.Empty;

            NoobWebClient client = new NoobWebClient();
            string json = await client.GetResponse("https://slack.com/api/rtm.start", RequestType.Post, "token", this.SlackKey);
            JObject jData = JObject.Parse(json);

            TeamID = jData["team"]["id"].Value<string>();
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

                string messageText = (jObject["text"] != null ? jObject["text"].Value<string>() : null);
                // check to see if bot has been mentioned
                SlackMessage message = new SlackMessage() {
                    ChatHub = hub,
                    MentionsBot = (messageText != null ? Regex.IsMatch(messageText, BotNameRegex, RegexOptions.IgnoreCase) : false),
                    RawData = json,
                    // some messages may not have text or a user (like unfurled data from URLs)
                    Text = messageText,
                    User = (jObject["user"] != null ? new SlackUser() { ID = jObject["user"].Value<string>() } : null)
                };

                ResponseContext context = new ResponseContext() {
                    BotHasResponded = false,
                    BotUserID = UserID,
                    BotUserName = UserName,
                    Message = message,
                    TeamID = this.TeamID,
                    UserNameCache = new ReadOnlyDictionary<string, string>(this.UserNameCache)
                };

                if (StaticResponseContextData != null) {
                    foreach (string key in StaticResponseContextData.Keys) {
                        context.Set(key, StaticResponseContextData[key]);
                    }
                }

                // margie can never respond to herself and requires that the message have text and be from an actual person
                if (message.User != null && message.User.ID != UserID && message.Text != null) {
                    foreach (IResponseProcessor processor in ResponseProcessors) {
                        if (processor.CanRespond(context)) {
                            await Say(processor.GetResponse(context), context);
                            context.BotHasResponded = true;
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

            if(message.ChatHub != null) {
                chatHubID = message.ChatHub.ID;
            }
            else if(context != null && context.Message.ChatHub != null) {
                chatHubID = context.Message.ChatHub.ID;
            }

            if(chatHubID != null) {
                NoobWebClient client = new NoobWebClient();
                await client.GetResponse(
                    "https://slack.com/api/chat.postMessage",
                    RequestType.Post,
                    "token", this.SlackKey,
                    "channel", chatHubID,
                    "text", message.Text,
                    "as_user", "true"
                );
            }
            else {
                throw new ArgumentException("When calling the Say() method, the message parameter must have its ChatHub property set.");
            }
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