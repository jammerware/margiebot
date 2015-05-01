using MargieBot.MessageProcessors;
using MargieBot.Models;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace MargieBot.UI.Infrastructure.BotResponseProcessors
{
    public class ScoreboardRequestMessageProcessor : IBotMentionedResponseProcessor
    {
        public bool CanRespond(MargieContext context)
        {
            return Regex.IsMatch(context.Message.Text, @"\bscore\b", RegexOptions.IgnoreCase);
        }

        public string GetResponse(MargieContext context)
        {
            if(context.ScoreContext.Scores.Count > 0) {
                StringBuilder builder = new StringBuilder(context.Phrasebook.GetScoreboardHype());
                builder.Append("```");

                // add the scores to a list for sorting. while we do, figure out who has the longest name for the pseudo table formatting
                List<KeyValuePair<string, int>> sortedScores = new List<KeyValuePair<string, int>>();
                string longestName = string.Empty;

                foreach(string key in context.ScoreContext.Scores.Keys) {
                    KeyValuePair<string, int> newScore = new KeyValuePair<string, int>(context.UserNameCache[key], context.ScoreContext.Scores[key]);
                    if(newScore.Key.Length > longestName.Length) {
                        longestName = newScore.Key;
                    }

                    sortedScores.Add(newScore);
                }
                sortedScores.Sort((x, y) => { return y.Value.CompareTo(x.Value); });

                foreach(KeyValuePair<string, int> userScore in sortedScores)  {
                    StringBuilder nameString = new StringBuilder(userScore.Key);
                    while(nameString.Length < longestName.Length) {
                        nameString.Append(" ");
                    }

                    builder.Append(nameString.ToString() + " | " + userScore.Value.ToString() + "\n");
                }

                builder.Append("```");
                return builder.ToString();
            }
            else { return "Not a one-of-ya has scored yet. Come on, sleepyheads!"; }
        }
    }
}