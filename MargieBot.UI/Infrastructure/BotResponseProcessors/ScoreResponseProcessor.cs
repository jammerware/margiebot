using MargieBot.MessageProcessors;
using MargieBot.Models;
using System.Text.RegularExpressions;

namespace MargieBot.UI.Infrastructure.BotResponseProcessors
{
    public class ScoreResponseProcessor : IResponseProcessor, IScoringProcessor
    {
        private static string SCORE_REGEX = @"(?<formattedUserID>\<@(?<userID>U[a-zA-Z0-9]+)\>)\s*\+\s*1";

        public bool CanRespond(MargieContext context)
        {
            return IsScoringMessage(context.Message);
        }

        public string GetResponse(MargieContext context)
        {
            Match userScored = Regex.Match(context.Message.Text, SCORE_REGEX);
            string userID = userScored.Groups["userID"].Value;
            string formattedUserID = userScored.Groups["formattedUserID"].Value;

            if (userID == context.Message.User) {
                return string.Format("Oh, honey. {0}, you can't score yourself! What kinda game would that be?! Y'all, {0} is cute, but I think he/she might be dumb as a box o' rocks.", formattedUserID);
            }
            else {
                int userScore = context.ScoreContext.GetUserScore(userID);

                if (userID == context.MargiesUserID) {
                    int margieScore = context.ScoreContext.GetUserScore(context.MargiesUserID);
                    return string.Format("Awwww, aren't you a sweetie! *[blushes]* If you insist. Now I have {0} point{1}.", margieScore, margieScore == 1 ? string.Empty : "s");
                }
                else if (context.ScoreContext.NewScoreResult != null && context.ScoreContext.NewScoreResult.UserID == userID) {
                    return string.Format("A new challenger appears, y'all! {0} is on the board with a point. {1}", formattedUserID, context.Phrasebook.GetAffirmation());
                }
                else {
                    return string.Format(
                        "{0} {1} just scored a point. {2} {1}, your score is now {3}.",
                        context.Phrasebook.GetExclamation(),
                        formattedUserID,
                        context.Phrasebook.GetAffirmation(),
                        userScore
                    );
                }
            }
        }

        public bool IsScoringMessage(SlackMessage message)
        {
            return message.User != Constants.SLACKBOTS_USERID && Regex.IsMatch(message.Text, SCORE_REGEX);
        }

        public ScoreResult Score(SlackMessage message)
        {
            Match userScored = Regex.Match(message.Text, SCORE_REGEX);
            string userID = userScored.Groups["userID"].Value;

            return new ScoreResult() {
                ScoreIncrement = (userID != message.User ? 1 : 0),
                UserID = userID
            };
        }
    }
}