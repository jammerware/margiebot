using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using MargieBot.Models;
using MargieBot.Responders;
using Newtonsoft.Json.Linq;

namespace MargieBot.ExampleResponders.Responders
{
    public class GoogleImageSearchResponder : IResponder
    {
        private const string RESPONSE_REGEX = @"show (me|us) (a|an )?(?<searchTerm>[\s\S]+)";

        public string ApiKey { get; private set; }
        public string SearchEngineId { get; private set; }

        public GoogleImageSearchResponder(string searchEngineId, string apiKey)
        {
            ApiKey = apiKey;
            SearchEngineId = searchEngineId;
        }

        public bool CanRespond(ResponseContext context)
        {
            return context.Message.MentionsBot && Regex.IsMatch(context.Message.Text, RESPONSE_REGEX);
        }

        public BotMessage GetResponse(ResponseContext context)
        {
            var searchTerm = Regex.Match(context.Message.Text, RESPONSE_REGEX).Groups["searchTerm"].Value;
            var requestUrl = $"https://www.googleapis.com/customsearch/v1?q={WebUtility.UrlEncode(searchTerm)}&cx={WebUtility.UrlEncode(SearchEngineId)}&key={WebUtility.UrlEncode(ApiKey)}&safe=medium&searchType=image";

            string results = new HttpClient().GetStringAsync(requestUrl).GetAwaiter().GetResult();
            var jObject = JObject.Parse(results);

            if(jObject["items"] != null)
            {
                var items = jObject["items"] as JArray;

                if(items.Count > 0)
                {
                    return new BotMessage()
                    {
                        Text = items[0].Value<string>("link")
                    };
                }
            }

            return new BotMessage()
            {
                Text = $@"Couldn't find any good pictures for *""{searchTerm}""*. Are you sure that's even a thing, darlin'?"
            };
        }
    }
}