using BazamWPF.UIHelpers;
using BazamWPF.ViewModels;
using MargieBot.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace MargieBot.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private Margie _Margie = new Margie();

        public ICommand ConnectCommand
        {
            get { 
                return new RelayCommand((timeForThings) => {
                    if (ConnectionStatus) {
                        _Margie.Disconnect();
                    }
                    else {
                        _Margie.Connect(); 
                    }
                }); 
            }
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

        public MainWindowViewModel()
        {
            _Margie.ConnectionStatusChanged += (bool isConnected) => {
                ConnectionStatus = isConnected;
            };

            _Margie.MessageReceived += (string message) => {
                while (_Messages.Count > 500) {
                    _Messages.RemoveAt(0);
                }
                _Messages.Add(message);
                RaisePropertyChanged("Messages");
            };
        }
    }
}