using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Authentication;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bazam.Http;
using MargieBot.Utilities;
using MargieBot.WebSockets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
                if (_BotNameRegex == string.Empty && IsConnected)
                {
                    _BotNameRegex = new BotNameRegexComposer().ComposeFor(UserName, UserID, Aliases);
                }

                return _BotNameRegex;
            }
            set { _BotNameRegex = value; }
        }

        private Dictionary<string, string> UserNameCache { get; set; } = new Dictionary<string, string>();
        private MargieBotWebSocket WebSocket { get; set; }
        private string SlackRtmStartHelp = "https://api.slack.com/methods/rtm.start";
        #endregion

        #region Public properties
        private IEnumerable<string> _Aliases = new List<string>();
        public IEnumerable<string> Aliases
        {
            get { return _Aliases; }
            set
            {
                _Aliases = value;
                BotNameRegex = string.Empty;
            }
        }

        // TODO: think about this
        //
        // This is a List<IResponder> because I wanted the end dev to be really free to manipulate the responders programmatically. Even IList doesn't have everything I wanted (AddRange, for example),
        // so I made it a List for now. 
        public List<IResponder> Responders { get; } = new List<IResponder>();

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
                if (_ConnectedSince != value)
                {
                    _ConnectedSince = value;
                    RaiseConnectionStatusChanged();
                }
            }
        }

        public Dictionary<string, object> ResponseContext { get; } = new Dictionary<string, object>();
        public string SlackKey { get; private set; }
        public string TeamID { get; private set; }
        public string TeamName { get; private set; }
        public string UserID { get; private set; }
        public string UserName { get; private set; }
        #endregion

        /// <summary>
        /// Connects this bot to Slack using the slack API key provided. Set yours up at https://yourteam.slack.com/apps/manage.
        /// </summary>
        /// <param name="slackKey">The API key the bot will use to identify itself to the Slack API.</param>
        public async Task Connect(string slackKey)
        {
            SlackKey = slackKey;

            // disconnect in case we're already connected like a crazy person
            Disconnect();

            // kill the regex for our bot's name - we'll rebuild it upon request with some of the info we get here
            BotNameRegex = string.Empty;

            // start session and get response
            var httpClient = new HttpClient();
            var json = await httpClient.GetStringAsync($"https://slack.com/api/rtm.start?token={SlackKey}");
            var jData = JObject.Parse(json);

            // Handle exceptions.

            if (! jData["ok"].Value<bool>())
            {
                var errorMessage = jData["ok"].Value<string>();
                switch (errorMessage)
                {       
                    case "not_authed":          
                    case "account_inactive":
                    case "invalid_auth":
                        InvalidCredentialException exIC = new InvalidCredentialException(errorMessage);
                        exIC.HelpLink = SlackRtmStartHelp;
                        throw exIC;
                    case "invalid_arg_name":
                    case "invalid_array_arg":
                    case "invalid_charset":
                    case "invalid_form_data":
                    case "invalid_post_type":
                    case "missing_post_type":
                        ArgumentException exAE = new ArgumentException(errorMessage);
                        exAE.HelpLink = SlackRtmStartHelp;
                        throw exAE;
                    case "request_timeout":
                        TimeoutException exTE = new TimeoutException(errorMessage);
                        exTE.HelpLink = SlackRtmStartHelp;
                        throw exTE;
                    default:
                      throw new Exception(errorMessage);
                }
            }

            // read various bot properties out of the response
            TeamID = jData["team"]["id"].Value<string>();
            TeamName = jData["team"]["name"].Value<string>();
            UserID = jData["self"]["id"].Value<string>();
            UserName = jData["self"]["name"].Value<string>();
            var webSocketUrl = jData["url"].Value<string>();

            // rebuild the username cache
            UserNameCache.Clear();
            foreach (JObject userObject in jData["users"])
            {
                UserNameCache.Add(userObject["id"].Value<string>(), userObject["name"].Value<string>());
            }

            // load the channels, groups, and DMs that margie's in
            Dictionary<string, SlackChatHub> hubs = new Dictionary<string, SlackChatHub>();
            ConnectedHubs = hubs;

            // channelz
            if (jData["channels"] != null)
            {
                foreach (JObject channelData in jData["channels"])
                {
                    if (!channelData["is_archived"].Value<bool>() && channelData["is_member"].Value<bool>())
                    {
                        var channel = new SlackChatHub()
                        {
                            ID = channelData["id"].Value<string>(),
                            Name = "#" + channelData["name"].Value<string>(),
                            Type = SlackChatHubType.Channel
                        };
                        hubs.Add(channel.ID, channel);
                    }
                }
            }

            // groupz
            if (jData["groups"] != null)
            {
                foreach (JObject groupData in jData["groups"])
                {
                    if (!groupData["is_archived"].Value<bool>() && groupData["members"].Values<string>().Contains(UserID))
                    {
                        var group = new SlackChatHub()
                        {
                            ID = groupData["id"].Value<string>(),
                            Name = groupData["name"].Value<string>(),
                            Type = SlackChatHubType.Group
                        };
                        hubs.Add(group.ID, group);
                    }
                }
            }

            // dmz
            if (jData["ims"] != null)
            {
                foreach (JObject dmData in jData["ims"])
                {
                    var userID = dmData["user"].Value<string>();
                    var dm = new SlackChatHub()
                    {
                        ID = dmData["id"].Value<string>(),
                        Name = "@" + (UserNameCache.ContainsKey(userID) ? UserNameCache[userID] : userID),
                        Type = SlackChatHubType.DM
                    };
                    hubs.Add(dm.ID, dm);
                }
            }

            // set up the websocket
            WebSocket = new MargieBotWebSocket();
            WebSocket.OnOpen += (object sender, EventArgs e) =>
            {
                // set connection-related properties
                ConnectedSince = DateTime.Now;
            };
            WebSocket.OnMessage += async (object sender, string message) =>
            {
                await ListenTo(message);
            };
            WebSocket.OnClose += (object sender, EventArgs e) =>
            {
                // set connection-related properties
                ConnectedSince = null;
                TeamID = null;
                TeamName = null;
                UserID = null;
                UserName = null;
            };
            
            // connect
            await WebSocket.Connect(webSocketUrl);
        }

        /// <summary>
        /// Disconnect this bot from Slack.
        /// </summary>
        public void Disconnect()
        {
            WebSocket?.Disconnect();
        }

        private async Task ListenTo(string json)
        {
            bool isValidJson = true;
            JObject jObject = null;
            try
            {
                jObject = JObject.Parse(json);
            }
            catch(JsonReaderException)
            {
                isValidJson = false;
#if DEBUG
                Console.WriteLine($"Illegal JSON message: {json}");
#endif
            }

            if (!isValidJson)
                return;

            if (jObject["type"]?.Value<string>() == "message")
            {
                var channelID = jObject["channel"]?.Value<string>();
                SlackChatHub hub = null;

                if (ConnectedHubs.ContainsKey(channelID))
                {
                    hub = ConnectedHubs[channelID];
                }
                else
                {
                    hub = SlackChatHub.FromID(channelID);
                    var hubs = new Dictionary<string, SlackChatHub>(ConnectedHubs.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
                    hubs.Add(hub.ID, hub);
                    ConnectedHubs = hubs;
                }

                // some messages may not have text or a user (like unfurled data from URLs)
                string messageText = jObject["text"]?.Value<string>();

                SlackUser user = jObject["user"] != null
                    ? new SlackUser()
                    {
                        ID = jObject["user"].Value<string>()
                    }
                    : null;

                if (user == null)
                {
                    // a back door (for tests) to respond message from incoming webhook as a person.
                    string userSlackIdFile = Path.GetFullPath("UserSlackId.debug");

#if DEBUG
                    Console.WriteLine($"User Slack ID file: '{userSlackIdFile}'");
#endif

                    if (File.Exists(userSlackIdFile))
                    {
                        string userSlackId = File.ReadAllText(userSlackIdFile, Encoding.UTF8).Trim();
                        if (!string.IsNullOrWhiteSpace(userSlackId))
                        {
                            user = new SlackUser()
                            {
                                ID = userSlackId
                            };
                        }
                    }
                }

                SlackMessage message = new SlackMessage()
                {
                    ChatHub = hub,
                    // check to see if bot has been mentioned
                    MentionsBot = messageText != null && Regex.IsMatch(messageText, BotNameRegex, RegexOptions.IgnoreCase),
                    RawData = json,
                    Text = messageText,
                    User = user
                };

                ResponseContext context = new ResponseContext()
                {
                    BotHasResponded = false,
                    BotUserID = UserID,
                    BotUserName = UserName,
                    Message = message,
                    TeamID = TeamID,
                    UserNameCache = new ReadOnlyDictionary<string, string>(UserNameCache)
                };

                // if the end dev has added any static entries to the ResponseContext collection of Bot, add them to the context being passed to the responders.
                if (ResponseContext != null)
                {
                    foreach (string key in ResponseContext.Keys)
                    {
                        context.Set(key, ResponseContext[key]);
                    }
                }

                // margie can never respond to herself and requires that the message have text and be from an actual person
                if (message.User != null && message.User.ID != UserID && message.Text != null)
                {
                    foreach (var responder in Responders ?? Enumerable.Empty<IResponder>())
                    {
                        if (responder != null && responder.CanRespond(context))
                        {
                            await SendIsTyping(message.ChatHub);
                            await Say(responder.GetResponse(context), context);
                            context.BotHasResponded = true;
                        }
                    }
                }
            }

            RaiseMessageReceived(json);
        }

        /// <summary>
        /// Allows you to programmatically operate the bot. The bot will post the passed message in whichever "chat hub" (DM, channel, or group) the message specifies.
        /// </summary>
        /// <param name="message">The message you want the bot to post in Slack.</param>
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

            if (message.ChatHub != null)
            {
                chatHubID = message.ChatHub.ID;
            }
            else if (context != null && context.Message.ChatHub != null)
            {
                chatHubID = context.Message.ChatHub.ID;
            }

            if(chatHubID == null)
            {
                throw new ArgumentException($"When calling the {nameof(Say)}() method, the {nameof(message)} parameter must have its {nameof(message.ChatHub)} property set.");
            }

            var client = new NoobWebClient();
            var values = new List<string>() {
                    "token", SlackKey,
                    "channel", chatHubID,
                    "text", message.Text,
                    "as_user", "true"
                };

            if (message.Attachments.Count > 0)
            {
                values.Add("attachments");
                values.Add(JsonConvert.SerializeObject(message.Attachments));
            }

            await client.DownloadString(
                "https://slack.com/api/chat.postMessage",
                RequestMethod.Post,
                values.ToArray()
            );
        }

        /// <summary>
        /// Causes the bot to appear as though it's typing in whichever hub you specify. Useful for custom interactions, but note that you 
        /// don't need to set this while typical responders are being evaluated - the Bot class does that for you.
        /// </summary>
        /// <param name="chatHub">The hub in which the bot should appear to be typing.</param>
        /// <returns></returns>
        public async Task SendIsTyping(SlackChatHub chatHub)
        {
            if(!IsConnected)
            {
                throw new InvalidOperationException(@"Can't send the ""Bot typing"" indicator when the bot is disconnected.");
            }

            var message = new {
                type = "typing",
                channel = chatHub.ID,
                user = UserID
            };

            await WebSocket.Send(JsonConvert.SerializeObject(message));
        }
        
        #region Events
        public event MargieConnectionStatusChangedEventHandler ConnectionStatusChanged;
        private void RaiseConnectionStatusChanged()
        {
            ConnectionStatusChanged?.Invoke(IsConnected);
        }

        public event MargieMessageReceivedEventHandler MessageReceived;
        private void RaiseMessageReceived(string debugText)
        {
            MessageReceived?.Invoke(debugText);
        }
        #endregion
    }
}