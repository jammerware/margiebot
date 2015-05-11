using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bazam.NoobWebClient;
using MargieBot.MessageProcessors;
using MargieBot.Models;
using Newtonsoft.Json.Linq;

namespace MargieBot.UI.Infrastructure.BotResponseProcessors.GW2ResponseProcessors
{
    public class WvWResponseProcessor : IResponseProcessor
    {
        public bool CanRespond(ResponseContext context)
        {
            return context.Message.MentionsBot && context.Message.Text.ToLower().Contains("borlis pass");
        }

        public async Task<BotMessage> GetResponse(ResponseContext context)
        {
            NoobWebClient client = new NoobWebClient();

            string worldsData = await client.GetResponse("https://api.guildwars2.com/v1/world_names.json", RequestType.Get);
            string matchesData = await client.GetResponse("https://api.guildwars2.com/v1/wvw/matches.json", RequestType.Get);

            // resolve relevant IDs and shit from these requests
            JArray worldsObject = JArray.Parse(worldsData);
            string bpWorldID = worldsObject.Where(world => world["name"].Value<string>() == "Borlis Pass").FirstOrDefault().Value<string>();
            
            // pull the details from BP's current match
            string matchDetailsData = await client.GetResponse("https://api.guildwars2.com/v1/wvw/match_details.json?match_id=", RequestType.Get);

            return new BotMessage() {
                Text = "It's a scorcher out there in WvW this week. Borlis Pass's world ID is " + bpWorldID + "."
            };
        }


        BotMessage IResponseProcessor.GetResponse(ResponseContext context)
        {
            throw new NotImplementedException();
        }
    }
}