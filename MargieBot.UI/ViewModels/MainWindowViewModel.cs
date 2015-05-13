using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Input;
using Bazam.WPF.UIHelpers;
using Bazam.WPF.ViewModels;
using MargieBot.MessageProcessors;
using MargieBot.Models;
using MargieBot.UI.Infrastructure.BotResponseProcessors;
using MargieBot.UI.Infrastructure.BotResponseProcessors.DnDResponseProcessors;
using MargieBot.UI.Infrastructure.BotResponseProcessors.GW2ResponseProcessors;
using MargieBot.UI.Infrastructure.Models;

namespace MargieBot.UI.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private Bot _Margie;

        private string _AuthKeySlack = string.Empty;
        public string AuthKeySlack
        {
            get { return _AuthKeySlack; }
            set { ChangeProperty<MainWindowViewModel>(vm => vm.AuthKeySlack, value); }
        }

        private IReadOnlyList<SlackChatHub> _ConnectedHubs;
        public IReadOnlyList<SlackChatHub> ConnectedHubs
        {
            get { return _ConnectedHubs; }
            set { ChangeProperty<MainWindowViewModel>(vm => vm.ConnectedHubs, value); }
        }

        private bool _ConnectionStatus = false;
        public bool ConnectionStatus
        {
            get { return _ConnectionStatus; }
            set { ChangeProperty<MainWindowViewModel>(vm => vm.ConnectionStatus, value); }
        }

        private List<string> _Messages = new List<string>();
        public IEnumerable<string> Messages
        {
            get { return _Messages; }
        }

        private string _MessageToSend = string.Empty;
        public string MessageToSend
        {
            get { return _MessageToSend; }
            set { ChangeProperty<MainWindowViewModel>(vm => vm.MessageToSend, value); }
        }

        private SlackChatHub _SelectedChatHub;
        public SlackChatHub SelectedChatHub
        {
            get { return _SelectedChatHub; }
            set { ChangeProperty<MainWindowViewModel>(vm => vm.SelectedChatHub, value); }
        }

        public ICommand ConnectCommand
        {
            get { 
                return new RelayCommand(async (timeForThings) => {
                    if (_Margie != null && ConnectionStatus) {
                        SelectedChatHub = null;
                        ConnectedHubs = null;
                        _Margie.Disconnect();
                    }
                    else {
                        // let's margie
                        _Margie = new Bot(AuthKeySlack);
                        _Margie.Aliases = GetAliases();
                        _Margie.ResponseContext = GetStaticResponseContextData();
                        
                        // PROCESSOR WIREUP
                        _Margie.ResponseProcessors.AddRange(GetResponseProcessors());

                        _Margie.ConnectionStatusChanged += (bool isConnected) => {
                            ConnectionStatus = isConnected;
                        };
                        _Margie.MessageReceived += (string message) => {
                            int messageCount = _Messages.Count - 500;
                            for (int i = 0; i < messageCount; i++) {
                                _Messages.RemoveAt(0);
                            }

                            _Messages.Add(message);
                            RaisePropertyChanged("Messages");
                        };

                        await _Margie.Connect(); 

                        // now that we're connected, build list of connected hubs for great glory
                        List<SlackChatHub> hubs = new List<SlackChatHub>();
                        hubs.AddRange(_Margie.ConnectedChannels);
                        hubs.AddRange(_Margie.ConnectedGroups);
                        hubs.AddRange(_Margie.ConnectedDMs);
                        ConnectedHubs = hubs;

                        if (ConnectedHubs.Count > 0) {
                            SelectedChatHub = ConnectedHubs[0];
                        }
                    }
                }); 
            }
        }

        public ICommand TalkCommand
        {
            get
            {
                return new RelayCommand(async (letsChatterYall) => {
                    await _Margie.Say(new BotMessage() { Text = MessageToSend, ChatHub = SelectedChatHub });
                    MessageToSend = string.Empty;
                });
            }
        }

        /// <summary>
        /// Replace the contents of the list returned from this method with any aliases you might want your bot to respond to. If you
        /// don't want your bot to respond to anything other than its actual name, just return an empty list here.
        /// </summary>
        /// <returns>A list of aliases that will cause the BotWasMentioned property of the ResponseContext to be true</returns>
        private IReadOnlyList<string> GetAliases()
        {
            return new List<string>() { "Margie" };
        }

        /// <summary>
        /// If you want to use this application to run your bot, here's where you start. Just scrap as many of the processors
        /// described in this method as you want and start fresh. Define your own resposne processors using the methods describe
        /// at https://github.com/jammerware/margiebot/wiki/Configuring-responses and return them in an IList<IResponseProcessor>. 
        /// Boom! You have your own bot.
        /// </summary>
        /// <returns>A list of the processors this bot should respond with.</returns>
        private IList<IResponseProcessor> GetResponseProcessors()
        {
            // Some of these are more complicated than they need to be for the sake of example
            List<IResponseProcessor> responseProcessors = new List<IResponseProcessor>();

            // examples of semi-complex or "messier" processors (created in separate classes)
            responseProcessors.Add(new ScoreResponseProcessor());
            responseProcessors.Add(new ScoreboardRequestResponseProcessor());
            responseProcessors.Add(new RollResponseProcessor());
            responseProcessors.Add(new CharacterResponseProcessor());
            responseProcessors.Add(new WeatherRequestResponseProcessor());
            responseProcessors.Add(new WvWResponseProcessor());
            responseProcessors.Add(new WhatsNewResponseProcessor());

            // examples of simple-ish "inline" processors
            // this processor hits on Slackbot when he talks 1/4 times or so
            _Margie.ResponseProcessors.Add(_Margie.CreateResponseProcessor(
                (ResponseContext context) => { return (context.Message.User.IsSlackbot && new Random().Next(4) <= 1); },
                (ResponseContext context) => { return context.Get<Phrasebook>().GetSlackbotSalutation(); }
            ));

            // easiest one of all - this one responds if someone thanks Margie
            responseProcessors.Add(_Margie.CreateResponseProcessor(
                (ResponseContext context) => { return context.Message.MentionsBot && Regex.IsMatch(context.Message.Text, @"\b(thanks|thank you)\b", RegexOptions.IgnoreCase); },
                (ResponseContext context) => { return context.Get<Phrasebook>().GetYoureWelcome(); }
            ));

            // example of Supa Fly Mega EZ Syntactic Sugary Response Processors (not their actual name)
            _Margie
                .RespondsTo("get on that")
                .With("Sure, hun!")
                .With("I'll see what I can do, sugar.")
                .With("I'll try. No promises, though!")
                .IfBotIsMentioned();

            // you can do these with regexes too
            _Margie
                .RespondsTo("what (can|do) you do", true)
                .With(@"Lots o' things! I mean, potentially, anyway. Right now I'm real good at keepin' score (try plus-one-ing one of your buddies sometime). I'm learnin' about how to keep up with the weather from my friend DonnaBot. I also can't quite keep my eyes off a certain other bot around here :) If there's anythin' else you think I can help y'all with, just say so! The feller who made me tends to keep an eye on me and see how I'm doin'. So there ya have it.")
                .IfBotIsMentioned();
            _Margie.RespondsTo("(how did|how'd) you").With("Well, promise you won't tell nobody, but I'm a HUGE CSI fan. I learned a trick from those fellers and created a GUI interface using Visual Basic to track the IP.").IfBotIsMentioned();

            // this last one just responds if someone says "hi" or whatever to Margie, but only if no other processor has responded
            responseProcessors.Add(_Margie.CreateResponseProcessor(
                (ResponseContext context) => {
                    return
                        context.Message.MentionsBot &&
                        !context.BotHasResponded &&
                        Regex.IsMatch(context.Message.Text, @"\b(hi|hey|hello|what's up|what's happening)\b", RegexOptions.IgnoreCase) &&
                        context.Message.User.ID != context.BotUserID &&
                        !context.Message.User.IsSlackbot;
                },
                (ResponseContext context) => {
                    return context.Get<Phrasebook>().GetQuery();
                }
            ));

            return responseProcessors;
        }

        /// <summary>
        /// If you want to share any data across all your processors, you can use the StaticResponseContextData property of the bot to do it. I elected
        /// to have most of my processors use a "Phrasebook" object to ensure a consistent tone across the bot's responses, so I stuff the Phrasebook
        /// into the context for use.
        /// </summary>
        /// <returns></returns>
        private static Dictionary<string, object> GetStaticResponseContextData()
        {
            return new Dictionary<string, object>() { 
                { "Phrasebook", new Phrasebook() }
            };
        }
    }
}