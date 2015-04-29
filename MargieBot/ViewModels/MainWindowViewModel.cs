using BazamWPF.UIHelpers;
using BazamWPF.ViewModels;
using MargieBot.Infrastructure;
using MargieBot.Infrastructure.Models;
using System.Collections.Generic;
using System.Windows.Input;

namespace MargieBot.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private Margie _Margie;

        private string _AuthKeySlack = string.Empty;
        public string AuthKeySlack
        {
            get { return _AuthKeySlack; }
            set { ChangeProperty<MainWindowViewModel>(vm => vm.AuthKeySlack, value); }
        }

        private string _AuthKeyWunderground = string.Empty;
        public string AuthKeyWunderground
        {
            get { return _AuthKeyWunderground; }
            set { ChangeProperty<MainWindowViewModel>(vm => vm.AuthKeyWunderground, value); }
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
                        _Margie = new Margie(AuthKeySlack);
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