using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Bazam.NoobWebClient;
using MargieBot.MessageProcessors;
using MargieBot.Models;
using MargieBot.UI.Infrastructure.Models.GW2;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MargieBot.UI.Infrastructure.BotResponseProcessors.GW2ResponseProcessors
{
    public class WvWResponseProcessor : IResponseProcessor
    {
        public bool CanRespond(ResponseContext context)
        {
            return context.Message.MentionsBot && Regex.IsMatch(context.Message.Text, @"\b(borlis pass|mist war)\b");
        }

        public BotMessage GetResponse(ResponseContext context)
        {
            NoobWebClient client = new NoobWebClient();

            string worldsData = client.GetResponse("https://api.guildwars2.com/v1/world_names.json", RequestType.Get).GetAwaiter().GetResult();
            string matchesData = client.GetResponse("https://api.guildwars2.com/v1/wvw/matches.json", RequestType.Get).GetAwaiter().GetResult();

            // resolve relevant IDs and shit from these requests
            Dictionary<int, string> worlds = new Dictionary<int, string>();
            foreach (JToken world in JArray.Parse(worldsData)) {
                worlds.Add(world["id"].Value<int>(), world["name"].Value<string>());
            }

            int bpWorldID = worlds.Where(w => w.Value == "Borlis Pass").First().Key;
            
            JToken tokenBPMatch = JObject.Parse(matchesData)["wvw_matches"].Where(
                m => 
                    m["red_world_id"].Value<int>() == bpWorldID ||
                    m["green_world_id"].Value<int>() == bpWorldID ||
                    m["blue_world_id"].Value<int>() == bpWorldID
            ).First();

            // pull the details from BP's current match
            string matchDetailsData = client.GetResponse("https://api.guildwars2.com/v1/wvw/match_details.json?match_id=" + tokenBPMatch["wvw_match_id"].Value<string>(), RequestType.Get).GetAwaiter().GetResult();
            WvWMatch match = JsonConvert.DeserializeObject<WvWMatch>(matchDetailsData);
            match.RedTeamID = tokenBPMatch["red_world_id"].Value<int>();
            match.BlueTeamID = tokenBPMatch["blue_world_id"].Value<int>();
            match.GreenTeamID = tokenBPMatch["green_world_id"].Value<int>();

            string messageText = null;
            WvWMatchStandings standings = match.GetStandings(worlds);

            if (standings.FirstPlace.World.ID == bpWorldID) {
                messageText = string.Format(
                    "It's a bright 'n' sunny day in the Mists, and it seems like Shamira's been at work. Borlis Pass is leadin' the pack. We're at {0:n0} over {1} with {2:n0} and {3} with {4:n0}. Git it done, BP!",
                    standings.FirstPlace.Points,
                    standings.SecondPlace.World.Name,
                    standings.SecondPlace.Points,
                    standings.ThirdPlace.World.Name,
                    standings.ThirdPlace.Points
                );
            }
            else if (standings.SecondPlace.World.ID == bpWorldID) {
                messageText = string.Format(
                    "Well, we're not on top of the dogpile, but we've been in worser scrapes than this. {0} leads the game with {1:n0} points, and we're behind 'em with {2:n0}. Poor {3} is in our dust with {4:n0}. Better suit up with Shami tonight!",
                    standings.FirstPlace.World.Name,
                    standings.FirstPlace.Points,
                    standings.SecondPlace.Points,
                    standings.ThirdPlace.World.Name,
                    standings.ThirdPlace.Points
                );
            }
            else {
                messageText = string.Format(
                    "It's not looking great, y'all :( Borlis Pass is takin' it square in the kisser. {0} leads the game with {1:n0} points, and {2} is behind 'em with {3:n0}. All we got is {4:n0}. Shamira Wolfstride needs us, y'all. Cancel your dates tonight.",
                    standings.FirstPlace.World.Name,
                    standings.FirstPlace.Points,
                    standings.SecondPlace.World.Name,
                    standings.SecondPlace.Points,
                    standings.ThirdPlace.Points
                );
            }

            return new BotMessage() {
                Text = messageText
            };
        }
    }
}