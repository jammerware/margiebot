using Bazam.NoobWebClient;
using MargieBot.MessageProcessors;
using MargieBot.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Text.RegularExpressions;

namespace MargieBot.UI.Infrastructure.BotResponseProcessors
{
    public class WeatherRequestResponseProcessor : IResponseProcessor
    {
        private const string WUNDERGROUND_API_KEY = "34c36fc5e831f2b9";
        private string _LastData = string.Empty;
        private DateTime? _LastDataGrab = null;

        public bool CanRespond(MargieContext context)
        {
            return Regex.IsMatch(context.Message.Text, @"\bweather\b");
        }

        public string GetResponse(MargieContext context)
        {
            string data = string.Empty;
            if (_LastDataGrab != null && _LastDataGrab.Value > DateTime.Now.AddMinutes(-10)) {
                data = _LastData;
            }
            else {
                NoobWebClient client = new NoobWebClient();
                data = client.GetResponse("http://api.wunderground.com/api/" + WUNDERGROUND_API_KEY + "/conditions/q/TN/Nashville.json", RequestType.Get).GetAwaiter().GetResult();
                _LastData = data;
                _LastDataGrab = DateTime.Now;
            }
            
            JObject jData = JObject.Parse(data);
            if (jData["current_observation"] != null) {
                string tempString = jData["current_observation"]["temp_f"].Value<string>();
                double temp = double.Parse(tempString);

                return "It's about " + Math.Round(temp).ToString() + "° out, and it's " + jData["current_observation"]["weather"].Value<string>().ToLower() + ". Not bad, but it ain't hoedown weather, is it?\n\nIf you wanna see more. head over to " + jData["current_observation"]["forecast_url"].Value<string>() + " - my girlfriend DonnaBot works over there!";
            }
            else {
                return "Aww, nuts. My weatherbot gal-pal ain't around. Try 'gin later - she's prolly just fixin' her makeup.";
            }
        }

        public bool ResponseRequiresBotMention(MargieContext context)
        {
            return true;
        }
    }
}