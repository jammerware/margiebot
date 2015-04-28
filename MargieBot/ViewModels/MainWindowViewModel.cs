using BazamWPF.UIHelpers;
using BazamWPF.ViewModels;
using MargieBot.Infrastructure;
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

        public ICommand ConnectCommand
        {
            get { 
                return new RelayCommand((timeForThings) => {
                    if (_Margie != null && ConnectionStatus) {
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

                        _Margie.Connect(); 
                    }
                }); 
            }
        }
    }
}