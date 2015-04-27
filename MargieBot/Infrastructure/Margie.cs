using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Bazam.NoobWebClient;
using MargieBot.Infrastructure.EventHandlers;
using MargieBot.Infrastructure.MessageProcessors;
using MargieBot.Infrastructure.Models;
using Newtonsoft.Json.Linq;
using WebSocketSharp;

namespace MargieBot.Infrastructure
{
    public class Margie
    {
        private Phrasebook Phrasebook { get; set; }
        private IList<IResponseProcessor> ResponseProcessors { get; set; }
        private IScoringProcessor ScoringProcessor { get; set; }
        private Scorebook Scorebook { get; set; }
        private string TeamID { get; set; }
        private string UserID { get; set; }
        private Dictionary<string, string> UserNameCache { get; set; }
        private WebSocket WebSocket { get; set; }

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

        public Margie()
        {
            // get the books ready
            Phrasebook = new Phrasebook();
            UserNameCache = new Dictionary<string, string>();

            // initialize the message processors
            // the debug one needs special setup
            DebugMessageProcessor debugProcessor = new DebugMessageProcessor();
            debugProcessor.OnDebugRequested += RaiseDebugRequested;

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

        public async void Connect()
        {
            // disconnect in case we're already connected like a crazy person
            Disconnect();

            NoobWebClient client = new NoobWebClient();
            string json = await client.GetResponse("https://slack.com/api/rtm.start", "token", Constants.AUTH_TOKEN);
            JObject jObject = JObject.Parse(json);

            TeamID = jObject["team"]["id"].Value<string>();
            UserID = jObject["self"]["id"].Value<string>();
            string webSocketUrl = jObject["url"].Value<string>();

            foreach (JObject userObject in jObject["users"]) {
                UserNameCache.Add(userObject["id"].Value<string>(), userObject["name"].Value<string>());
            }

            // start up scorebook for this team
            Scorebook = new Scorebook(TeamID);

            // set up the websocket and connect
            WebSocket = new WebSocket(webSocketUrl);
            WebSocket.OnClose += (object sender, CloseEventArgs e) => {
                IsConnected = false;
            };
            WebSocket.OnMessage += (object sender, MessageEventArgs args) => {
                ListenTo(args.Data);
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

        private void ListenTo(string json)
        {
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
        public event MargieConnectionStatusChangedEventHandler OnConnectionStatusChanged;
        private void RaiseConnectionStatusChanged()
        {
            if (OnConnectionStatusChanged != null) {
                OnConnectionStatusChanged(IsConnected);
            }
        }

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