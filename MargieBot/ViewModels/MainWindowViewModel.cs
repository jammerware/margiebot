using BazamWPF.UIHelpers;
using BazamWPF.ViewModels;
using MargieBot.Infrastructure;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Input;
using WebSocketSharp;

namespace MargieBot.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private WebSocket _WebSocket = null;
        private string _WebSocketUrl = string.Empty;
        private Margie _Margie = new Margie();

        public ICommand ConnectCommand
        {
            get {
                return new RelayCommand(async (timeToParty) => {
                    _WebSocketUrl = await _Margie.GetSocketUrl();
                    StartWebSocket();
                });
            }
        }
        
        public ICommand DisconnectCommand
        {
            get
            {
                return new RelayCommand((timeForThings) => {
                    StopWebSocket();
                });
            }
        }

        private string _Message;
        public string Message
        {
            get { return _Message; }
            set { ChangeProperty<MainWindowViewModel>(vm => vm.Message, value); }
        }

        private void StartWebSocket()
        {
            StopWebSocket();

            _WebSocket = new WebSocket(_WebSocketUrl);
            _WebSocket.OnMessage += (object sender, MessageEventArgs args) => {
                _Margie.ListenTo(args.Data);
            };
            _WebSocket.Connect();
            Message = "Connected";
        }

        private void StopWebSocket()
        {
            if (_WebSocket != null && _WebSocket.IsAlive) {
                _WebSocket.Close();
                Message = "Disconnected";
            }
        }

        private void Margie_DebugRequested(string debugMessage, string completeJson)
        {
            Message = debugMessage;
            if (completeJson != string.Empty) {
                Message += Environment.NewLine + completeJson;
            }
        }

        public MainWindowViewModel()
        {
            _Margie.OnDebugRequested += Margie_DebugRequested;
            Message = "Disconnected";
        }
    }
}