using MargieBot.Infrastructure.Models;
using System;
using System.Text.RegularExpressions;

namespace MargieBot.Infrastructure.MessageProcessors
{
    public class ScoreResponseProcessor : IResponseProcessor
    {
        private static string SCORE_REGEX = @"(?<formattedUserID>\<@(?<userID>U[a-zA-Z0-9]+)\>)\s*\+\s*1";

        public bool CanRespond(MargieContext context)
        {
            return context.Message.User != Constants.USER_SLACKBOT && Regex.IsMatch(context.Message.Text, SCORE_REGEX);
        }

        public string GetResponse(MargieContext context)
        {
            Match userScored = Regex.Match(context.Message.Text, SCORE_REGEX);
            string userID = userScored.Groups["userID"].Value;
            string formattedUserID = userScored.Groups["formattedUserID"].Value;

            if (userID == context.Message.User) {
                return string.Format("Oh, honey. {0}, you can't score yourself! What kinda game would this be?! Y'all, {0} is cute, but I think he/she might be dumb as a box o' rocks.", formattedUserID);
            }
            else {
                //margie.Scorebook.ScoreUser(userID, 1);
                //int userScore = margie.Scorebook.GetUserScore(userID);

                if (userID == context.MargiesUserID) {
                    return string.Format("Awwww, aren't you a sweetie! *[blushes]* If you insist. Now I have {0} point{1}.", context.UserContext.Score, context.UserContext.Score == 1 ? string.Empty : "s");
                }
                else if (!context.UserContext.HasScoredPreviously) {
                    return string.Format("A new challenger appears, y'all! {0} is on the board with a point. {1}", formattedUserID, context.Phrasebook.GetAffirmation());
                }
                else {
                    return string.Format(
                        "{0} {1} just scored a point. {2} {1}, your score is now {3}.",
                        context.Phrasebook.GetExclamation(),
                        formattedUserID,
                        context.Phrasebook.GetAffirmation(),
                        context.UserContext.Score
                    );
                }
            }
        }
    }
}