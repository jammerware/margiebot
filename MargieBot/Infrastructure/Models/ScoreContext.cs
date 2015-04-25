using System.Collections.Generic;

namespace MargieBot.Infrastructure.Models
{
    public class ScoreContext
    {
        public ScoreResult NewScoreResult { get; set; }
        public IReadOnlyDictionary<string, int> Scores { get; set; }
        
        public int GetUserScore(string userID)
        {
            if (Scores.ContainsKey(userID)) {
                return Scores[userID];
            }
            return 0;
        }
    }
}