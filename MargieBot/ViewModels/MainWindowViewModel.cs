using BazamWPF.UIHelpers;
using BazamWPF.ViewModels;
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

        public ICommand ConnectCommand
        {
            get {
                return new RelayCommand((timeToParty) => {
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
            _WebSocket = new WebSocket(_WebSocketUrl);
            _WebSocket.OnMessage += (object sender, MessageEventArgs args) => {
                Message = args.Data.ToString();
            };
            _WebSocket.Connect();
        }

        private void StopWebSocket()
        {
            if (_WebSocket != null && _WebSocket.IsAlive) {
                _WebSocket.Close();
                Message = "Disconnected";
            }
        }

        // xoxb-4597209409-Sy4JJEX6GblzmKrdF9mPngy7
        // xoxb-4599190677-HJTfW7q5O4hwaBqMBbEl4RBG
        public MainWindowViewModel()
        {
            Message = "Disconnected";
            WebRequest request = WebRequest.Create("https://slack.com/api/rtm.start");
            byte[] body = Encoding.UTF8.GetBytes("token=xoxb-4599190677-HJTfW7q5O4hwaBqMBbEl4RBG");
            request.Method = "POST";
            request.ContentLength = body.Length;
            request.ContentType = "application/x-www-form-urlencoded";
            using(BinaryWriter writer = new BinaryWriter(request.GetRequestStream())) {
                writer.Write(body);
            }

            string responseJson = string.Empty;
            WebResponse response = request.GetResponse();
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {
                responseJson = reader.ReadToEnd();
            }

            JObject jObject = JObject.Parse(responseJson);
            _WebSocketUrl = jObject["url"].Value<string>();
        }
    }
}