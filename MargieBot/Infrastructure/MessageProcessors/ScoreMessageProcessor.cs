using MargieBot.Infrastructure.Models;
using System;
using System.Text.RegularExpressions;

namespace MargieBot.Infrastructure.MessageProcessors
{
    public class ScoreMessageProcessor : IMessageProcessor
    {
        private static string SCORE_REGEX = @"(?<formattedUserID>\<@(?<userID>U[a-zA-Z0-9]+)\>)\s*\+\s*1";

        public bool IsRelevant(SlackMessage message, Margie margie, bool hasBeenRespondedTo)
        {
            return Regex.IsMatch(message.Text, SCORE_REGEX);
        }

        public void Respond(SlackMessage message, Margie margie)
        {
            Match userScored = Regex.Match(message.Text, SCORE_REGEX);
            string userID = userScored.Groups["userID"].Value;
            string formattedUserID = userScored.Groups["formattedUserID"].Value;
            bool hasUserScored = margie.Scorebook.HasUserScored(userID);

            if (userID == message.User) {
                string text = string.Format("Oh, honey. {0}, you can't score yourself! What kinda game would this be?! Y'all, {0} is cute, but I think he/she might be dumb as a box o' rocks.", formattedUserID);
                margie.Say(text, message.Channel);
            }
            else {
                margie.Scorebook.ScoreUser(userID, 1);
                int userScore = margie.Scorebook.GetUserScore(userID);

                if (userID == margie.UserID) {
                    string text = string.Format("Awwww, aren't you a sweetie! *[blushes]* If you insist. Now I have {0} point{1}.", userScore, userScore == 1 ? string.Empty : "s");
                    margie.Say(text, message.Channel);
                }
                else if (!hasUserScored) {
                    string text = string.Format("A new challenger appears, y'all! {0} is on the board with a point. {1}", formattedUserID, margie.GetAffirmation());
                    margie.Say(text, message.Channel);
                }
                else {
                    string text = string.Format(
                        "{0} {1} just scored a point. {2} {1}, your score is now {3}.",
                        margie.GetExclamation(),
                        formattedUserID,
                        margie.GetAffirmation(),
                        userScore
                    );
                    margie.Say(text, message.Channel);
                }
            }
        }
    }
}