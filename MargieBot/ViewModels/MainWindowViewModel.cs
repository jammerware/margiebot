using BazamWPF.UIHelpers;
using BazamWPF.ViewModels;
using MargieBot.Infrastructure;
using System;
using System.Windows.Input;

namespace MargieBot.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private Margie _Margie = new Margie();

        public ICommand ConnectCommand
        {
            get { return new RelayCommand((timeForThings) => { _Margie.Connect(); }); }
        }
        
        public ICommand DisconnectCommand
        {
            get { return new RelayCommand((timeForThings) => { _Margie.Disconnect(); }); }
        }

        private string _Message;
        public string Message
        {
            get { return _Message; }
            set { ChangeProperty<MainWindowViewModel>(vm => vm.Message, value); }
        }

        public MainWindowViewModel()
        {
            _Margie.OnConnectionStatusChanged += (bool isConnected) => {
                Message = isConnected ? "Connected" : "Disconnected";
            };

            _Margie.OnDebugRequested += (string debugMessage, string completeJson) => {
                Message = debugMessage;
                if (completeJson != string.Empty) {
                    Message += Environment.NewLine + completeJson;
                }
            };

            Message = "Disconnected";
        }
    }
}