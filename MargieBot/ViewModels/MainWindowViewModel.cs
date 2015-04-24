using Alchemy;
using Alchemy.Classes;
using BazamWPF.UIHelpers;
using BazamWPF.ViewModels;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MargieBot.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private WebSocketClient _Client = null;
        private string _WebSocketUrl = string.Empty;

        public ICommand CallCommand
        {
            get {
                return new RelayCommand((timeToParty) => {
                    _Client = new WebSocketClient(_WebSocketUrl);
                    _Client.OnReceive = OnReceive;
                    _Client.Connect();
                });
            }
        }

        private string _Message;
        public string Message
        {
            get { return _Message; }
            set { ChangeProperty<MainWindowViewModel>(vm => vm.Message, value); }
        }

        

        private void OnReceive(UserContext data)
        {
            Message = data.Data.ToString();
        }

        // xoxb-4597209409-Sy4JJEX6GblzmKrdF9mPngy7
        // xoxb-4599190677-HJTfW7q5O4hwaBqMBbEl4RBG
        public MainWindowViewModel()
        {
            Message = "Starting up...";
            WebRequest request = WebRequest.Create("https://slack.com/api/rtm.start");
            byte[] body = Encoding.UTF8.GetBytes("xoxb-4599190677-HJTfW7q5O4hwaBqMBbEl4RBG");
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