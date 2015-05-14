using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace MargieBot.UI.Infrastructure.Models.GW2
{
    public class WvWMatch
    {
        [JsonProperty(PropertyName = "maps")]
        public WvWMap[] Maps { get; set; }

        [JsonProperty(PropertyName = "match_id")]
        public string MatchID { get; set; }

        [JsonProperty(PropertyName = "scores")]
        public int[] Scores { get; set; }

        public int RedTeamID { get; set; }
        public int GreenTeamID { get; set; }
        public int BlueTeamID { get; set; }

        private int GetIDForColor(WvWColor color)
        {
            switch (color) {
                case WvWColor.Blue:
                    return BlueTeamID;
                case WvWColor.Green:
                    return GreenTeamID;
            }
            return RedTeamID;
        }

        private WvWColor GetTeamInPlace(int place)
        {
            int[] sortedScores = Scores.OrderByDescending(s => s).ToArray();
            int maxScore = Scores.Max();

            if (GetTeamScore(WvWColor.Red) == sortedScores[place]) {
                return WvWColor.Red;
            }
            else if (GetTeamScore(WvWColor.Blue) == sortedScores[place]) {
                return WvWColor.Blue;
            }
            else {
                return WvWColor.Green;
            }
        }

        private int GetTeamScore(WvWColor team)
        {
            // a little bizarrely, the service gives scores back in RBG order
            switch (team) {
                case WvWColor.Red:
                    return Scores[0];
                case WvWColor.Blue:
                    return Scores[1];
                case WvWColor.Green:
                    return Scores[2];
            }
            return 0;
        }

        public WvWMatchStandings GetStandings(Dictionary<int, string> teamDictionary)
        {
            WvWWorld firstWorld = new WvWWorld();
            firstWorld.Color = GetTeamInPlace(0);
            firstWorld.ID = GetIDForColor(firstWorld.Color);
            firstWorld.Name = teamDictionary[firstWorld.ID];

            WvWWorld secondWorld = new WvWWorld();
            secondWorld.Color = GetTeamInPlace(1);
            secondWorld.ID = GetIDForColor(secondWorld.Color);
            secondWorld.Name = teamDictionary[secondWorld.ID];

            WvWWorld thirdWorld = new WvWWorld(); // haha
            thirdWorld.Color = GetTeamInPlace(2);
            thirdWorld.ID = GetIDForColor(thirdWorld.Color);
            thirdWorld.Name = teamDictionary[thirdWorld.ID];

            return new WvWMatchStandings() {
                FirstPlace = new WvWWorldStanding() {
                    World = firstWorld,
                    Points = GetTeamScore(firstWorld.Color)
                },
                SecondPlace = new WvWWorldStanding() {
                    World = secondWorld,
                    Points = GetTeamScore(secondWorld.Color)
                },
                ThirdPlace = new WvWWorldStanding() {
                    World = thirdWorld,
                    Points = GetTeamScore(thirdWorld.Color)
                }
            };
        }
    }
}