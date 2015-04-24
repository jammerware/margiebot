using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace MargieBot.Infrastructure.Models
{
    public class Scorebook
    {
        private Dictionary<string, int> _TheScore = null;

        public Scorebook()
        {
            _TheScore = new Dictionary<string,int>();
            if (File.Exists(Constants.SCOREBOOK_FILEPATH)) {
                _TheScore = JsonConvert.DeserializeObject<Dictionary<string, int>>(File.ReadAllText(Constants.SCOREBOOK_FILEPATH));
            }
        }

        public int GetUserScore(string userID)
        {
            if (_TheScore.ContainsKey(userID)) {
                return _TheScore[userID];
            }
            return 0;
        }

        public bool HasUserScored(string userID)
        {
            return _TheScore.ContainsKey(userID);
        }

        private void Save()
        {
            // TODO: exception handling, y'all
            File.WriteAllText(Constants.SCOREBOOK_FILEPATH, JsonConvert.SerializeObject(_TheScore));
        }

        public void ScoreUser(string userID, int scoreValue)
        {
            if (_TheScore.ContainsKey(userID)) {
                _TheScore[userID] += scoreValue;
            }
            else {
                _TheScore.Add(userID, scoreValue);
            }
            Save();
        }
    }
}