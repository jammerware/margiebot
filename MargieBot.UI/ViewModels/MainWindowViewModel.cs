using BazamWPF.UIHelpers;
using BazamWPF.ViewModels;
using MargieBot.Extensions;
using MargieBot.MessageProcessors;
using MargieBot.Models;
using MargieBot.UI.Infrastructure.BotResponseProcessors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Input;

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
                        
                        // wire up some response processors
                        // the debug one needs special setup
                        DebugResponseProcessor debugProcessor = new DebugResponseProcessor();
                        debugProcessor.OnDebugRequested += (string debugText) => {
                            File.WriteAllText(DateTime.Now.Ticks.ToString(), debugText);
                        };

                        // also the ScoreResponseProcessor is pulling double duty as the ScoringProcessor
                        _Margie.ScoringProcessor = new ScoreResponseProcessor();

                        List<IResponseProcessor> responseProcessors = new List<IResponseProcessor>();

                        // examples of semi-complex or "messier" processors (created in separate classes)
                        responseProcessors.Add((IResponseProcessor)_Margie.ScoringProcessor);
                        responseProcessors.Add(debugProcessor);
                        responseProcessors.Add(new ScoreboardRequestMessageProcessor());
                        responseProcessors.Add(new WeatherRequestResponseProcessor());
                        responseProcessors.Add(new WhatDoYouDoResponseProcessor());
                        responseProcessors.Add(new WhatsNewResponseProcessor());

                        // examples of smooth operator stupid processors
                        _Margie.RespondsTo("say \"yay!\", margie!").With("Yay!");

                        // examples of simple-ish "inline" processors
                        // this processor hits on Slackbot when he talks 1/4 times or so
                        responseProcessors.Add(MessageProcessorHelper.Create(
                            (MargieContext context) => { return (context.Message.User == "USLACKBOT" && new Random().Next(4) <= 1); },
                            (MargieContext context) => { return context.Phrasebook.GetSlackbotSalutation(); },
                            false
                        ));
                        // this one just responds if someone says "hi" or whatever to Margie
                        responseProcessors.Add(MessageProcessorHelper.Create(
                            (MargieContext context) => {
                                return 
                                    Regex.IsMatch(context.Message.Text, @"\b(hi|hey|hello)\b", RegexOptions.IgnoreCase) &&
                                    context.Message.User != context.MargiesUserID &&
                                    context.Message.User != Constants.USER_SLACKBOT;
                            },
                            (MargieContext context) => {
                                return context.Phrasebook.GetQuery();
                            }
                        ));
                        // easiest one of all - this one responds if someone thanks Margie
                        responseProcessors.Add(MessageProcessorHelper.Create(
                            (MargieContext context) => { return Regex.IsMatch(context.Message.Text, @"\b(thanks|thank you)\b", RegexOptions.IgnoreCase); },
                            (MargieContext context) => { return context.Phrasebook.GetYoureWelcome(); }
                        ));

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