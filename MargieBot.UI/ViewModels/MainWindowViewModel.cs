using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Input;
using BazamWPF.UIHelpers;
using BazamWPF.ViewModels;
using MargieBot.MessageProcessors;
using MargieBot.Models;
using MargieBot.UI.Infrastructure.BotResponseProcessors;

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
                        _Margie.Pseudonyms = new List<string>() { "Margie" };
                        
                        // PROCESSOR WIREUP
                        // Some of these are more complicated than they need to be for the sake of example
                        // the debug one needs special setup
                        List<IResponseProcessor> responseProcessors = new List<IResponseProcessor>();

                        // also the ScoreResponseProcessor is pulling double duty as the ScoringProcessor
                        // going to modify the framework so this isn't a special case at some point soon
                        _Margie.ScoringProcessor = new ScoreResponseProcessor();

                        DebugResponseProcessor debugProcessor = new DebugResponseProcessor();
                        debugProcessor.OnDebugRequested += (string debugText) => {
                            File.WriteAllText(DateTime.Now.Ticks.ToString(), debugText);
                        };

                        // examples of semi-complex or "messier" processors (created in separate classes)
                        responseProcessors.Add((IResponseProcessor)_Margie.ScoringProcessor);
                        responseProcessors.Add(debugProcessor);
                        responseProcessors.Add(new ScoreboardRequestMessageProcessor());
                        responseProcessors.Add(new WeatherRequestResponseProcessor());
                        responseProcessors.Add(new WhatDoYouDoResponseProcessor());
                        responseProcessors.Add(new WhatsNewResponseProcessor());

                        // examples of simple-ish "inline" processors
                        // this processor hits on Slackbot when he talks 1/4 times or so
                        responseProcessors.Add(_Margie.CreateResponseProcessor(
                            (ResponseContext context) => { return (context.Message.UserID == Constants.SLACKBOTS_USERID && new Random().Next(4) <= 1); },
                            (ResponseContext context) => { return context.Phrasebook.GetSlackbotSalutation(); }
                        ));

                        // this one just responds if someone says "hi" or whatever to Margie
                        responseProcessors.Add(_Margie.CreateResponseProcessor(
                            (ResponseContext context) => {
                                return 
                                    context.Message.MentionsBot &&
                                    Regex.IsMatch(context.Message.Text, @"\b(hi|hey|hello)\b", RegexOptions.IgnoreCase) &&
                                    context.Message.UserID != context.BotUserID &&
                                    context.Message.UserID != Constants.SLACKBOTS_USERID;
                            },
                            (ResponseContext context) => {
                                return context.Phrasebook.GetQuery();
                            }
                        ));

                        // easiest one of all - this one responds if someone thanks Margie
                        responseProcessors.Add(_Margie.CreateResponseProcessor(
                            (ResponseContext context) => { return context.Message.MentionsBot && Regex.IsMatch(context.Message.Text, @"\b(thanks|thank you)\b", RegexOptions.IgnoreCase); },
                            (ResponseContext context) => { return context.Phrasebook.GetYoureWelcome(); }
                        ));

                        // example of a smooth operator processor
                        _Margie
                            .RespondsTo("get on that")
                            .With("Sure, hun!")
                            .With("I'll see what I can do, sugar.")
                            .With("I'll try. No promises, though!")
                            .IfBotIsMentioned();

                        // and that's how to create processors and add them to your MargieBot
                        _Margie.ResponseProcessors.AddRange(responseProcessors);

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
                    await _Margie.Say(MessageToSend, SelectedChatHub);
                    MessageToSend = string.Empty;
                });
            }
        }
    }
}