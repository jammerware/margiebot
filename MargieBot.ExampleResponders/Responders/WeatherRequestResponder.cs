using System;
using System.Configuration;
using System.IO;
using System.Text.RegularExpressions;
using Bazam.NoobWebClient;
using MargieBot.ExampleResponders.Models;
using MargieBot.Models;
using MargieBot.Responders;
using Newtonsoft.Json.Linq;

namespace MargieBot.ExampleResponders.Responders
{
    public class WeatherRequestResponder : IResponder
    {
        private string LastData { get; set; }
        private DateTime? LastDataGrab { get; set; }
        private string WundergroundAPIKey { get; set; }

        public WeatherRequestResponder(string apiKey)
        {
            //WundergroundAPIKey = ConfigurationManager.AppSettings["wundergroundApiKey"];
            WundergroundAPIKey = apiKey;
        }

        public bool CanRespond(ResponseContext context)
        {
            return (context.Message.MentionsBot || context.Message.ChatHub.Type == SlackChatHubType.DM) && Regex.IsMatch(context.Message.Text, @"\bweather\b");
        }

        public BotMessage GetResponse(ResponseContext context)
        {
            string data = string.Empty;
            if (LastDataGrab != null && LastDataGrab.Value > DateTime.Now.AddMinutes(-10)) {
                data = LastData;
            }
            else {
                NoobWebClient client = new NoobWebClient();
                data = client.GetResponse("http://api.wunderground.com/api/" + WundergroundAPIKey + "/conditions/q/TN/Nashville.json", RequestMethod.Get).GetAwaiter().GetResult();
                LastData = data;
                LastDataGrab = DateTime.Now;
            }
            
            JObject jData = JObject.Parse(data);
            if (jData["current_observation"] != null) {
                string tempString = jData["current_observation"]["temp_f"].Value<string>();
                double temp = double.Parse(tempString);

                return new BotMessage() { Text = "It's about " + Math.Round(temp).ToString() + "° out, and it's " + jData["current_observation"]["weather"].Value<string>().ToLower() + ". " + context.Get<Phrasebook>().GetWeatherAnalysis(temp) + "\n\nIf you wanna see more. head over to " + jData["current_observation"]["forecast_url"].Value<string>() + " - my girlfriend DonnaBot works over there!" };
            }
            else {
                return new BotMessage() { Text = "Aww, nuts. My weatherbot gal-pal ain't around. Try 'gin later - she's prolly just fixin' her makeup." };
            }
        }
    }
}