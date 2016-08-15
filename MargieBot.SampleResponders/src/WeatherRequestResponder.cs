using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Bazam.Http;
using MargieBot.SampleResponders.Models;
using Newtonsoft.Json.Linq;

namespace MargieBot.SampleResponders
{
    public class WeatherRequestResponder : IResponder
    {
        private const string WEATHER_NOTERM_REGEX = @"\bweather\b";

        private const string WEATHER_LOCATIONTERM_REGEX = @"\bweather\b\s+(?<cityTerm>\w+|\w+\s\w+)[,]\s+(?<stateTerm>\w{2})";

        private Dictionary<string, string> WeatherLookupCache;

        private string WundergroundAPIKey { get; set; }

        public WeatherRequestResponder(string apiKey)
        {
            WeatherLookupCache = new Dictionary<string, string>();
            WundergroundAPIKey = apiKey;
        }

        public bool CanRespond(ResponseContext context)
        {
            return (context.Message.MentionsBot || context.Message.ChatHub.Type == SlackChatHubType.DM)
                && (Regex.IsMatch(context.Message.Text, WEATHER_NOTERM_REGEX) || Regex.IsMatch(context.Message.Text, WEATHER_LOCATIONTERM_REGEX));
        }

        public BotMessage GetResponse(ResponseContext context)
        {
            string city = Regex.Match(context.Message.Text, WEATHER_LOCATIONTERM_REGEX).Groups["cityTerm"].Value;
            string state = Regex.Match(context.Message.Text, WEATHER_LOCATIONTERM_REGEX).Groups["stateTerm"].Value;
            string weatherReport = string.Empty;

            if (!string.IsNullOrEmpty(city) && !string.IsNullOrEmpty(state))
            {
                if (WeatherLookupCache.ContainsKey(city + state))
                {
                    WeatherLookupCache.TryGetValue(city + state, out weatherReport);
                }

                JObject jData = null;

                if (!string.IsNullOrEmpty(weatherReport))
                {
                    jData = JObject.Parse(weatherReport);

                    DateTime weatherReportAgeDateTimeStamp = jData["current_observation"]["local_time_rfc822"].Value<DateTime>();

                    if (weatherReportAgeDateTimeStamp < DateTime.Now.AddMinutes(-10))
                    {
                        WeatherLookupCache.Remove(city + state);

                        weatherReport = GetWeatherReport(city, state);

                        WeatherLookupCache.Add(city + state, weatherReport);
                    }
                }
                else
                {
                    weatherReport = GetWeatherReport(city, state);

                    WeatherLookupCache.Add(city + state, weatherReport);
                }

                if(jData == null)
                {
                    jData = JObject.Parse(weatherReport);
                }

                if (jData["current_observation"] != null)
                {
                    string tempString = jData["current_observation"]["temp_f"].Value<string>();
                    double temp = double.Parse(tempString);

                    return new BotMessage() { Text = "It's about " + Math.Round(temp).ToString() + "° out, and it's " + jData["current_observation"]["weather"].Value<string>().ToLower() + ". " + context.Get<Phrasebook>().GetWeatherAnalysis(temp) + "\n\nIf you wanna see more. head over to " + jData["current_observation"]["forecast_url"].Value<string>() + " - my girlfriend DonnaBot works over there!" };
                }
                else {
                    return new BotMessage() { Text = "Aww, nuts. My weatherbot gal-pal ain't around. Try 'gin later - she's prolly just fixin' her makeup." };
                }
            }
            else
            {
                // Default respone when request is not both a city and state
                return new BotMessage() { Text = "Shucks, hun. I don't think I understood. Can you try again? Make sure to tell me what city and state you want to hear about the weather from!" };
            }
        }

        protected string GetWeatherReport(string city, string state)
        {
            string resultWeatherReport = string.Empty;

            NoobWebClient client = new NoobWebClient();

            string requestUrl = string.Format("http://api.wunderground.com/api/{0}/conditions/q/{1}/{2}.json", WundergroundAPIKey, state.ToUpperInvariant(), city.Replace(' ', '_'));

            resultWeatherReport = client.DownloadString(requestUrl, RequestMethod.Get).GetAwaiter().GetResult();

            return resultWeatherReport;
        }
    }
}