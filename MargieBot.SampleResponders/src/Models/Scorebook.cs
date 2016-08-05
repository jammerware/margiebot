using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Newtonsoft.Json;

namespace MargieBot.SampleResponders.Models
{
    public class Scorebook
    {
        private Dictionary<string, int> Scores { get; set; }
        private string TeamID { get; set; }

        public Scorebook(string teamID)
        {
            Scores = new Dictionary<string, int>();
            this.TeamID = teamID;
            string filePath = GetFilePath();

            if (File.Exists(filePath)) {
                Scores = JsonConvert.DeserializeObject<Dictionary<string, int>>(File.ReadAllText(filePath));
            }
        }

        private string GetFilePath()
        {
            return TeamID + ".json";
        }

        public IReadOnlyDictionary<string, int> GetScores()
        {
            return new ReadOnlyDictionary<string, int>(Scores);
        }

        public int GetUserScore(string userID)
        {
            if (Scores.ContainsKey(userID)) {
                return Scores[userID];
            }
            return 0;
        }

        public bool HasUserScored(string userID)
        {
            return Scores.ContainsKey(userID);
        }

        public void ScoreUser(string userID, int increment)
        {
            ScoreUsers(new string[] { userID }, increment);
        }

        public void ScoreUsers(IEnumerable<string> userIDs, int increment)
        {
            foreach (string userID in userIDs) {
                if (Scores.ContainsKey(userID)) {
                    Scores[userID] += increment;
                }
                else {
                    Scores.Add(userID, increment);
                }
            }
            Save();
        }

        private void Save()
        {
            // TODO: exception handling, y'all
            File.WriteAllText(GetFilePath(), JsonConvert.SerializeObject(Scores));
        }
    }
}