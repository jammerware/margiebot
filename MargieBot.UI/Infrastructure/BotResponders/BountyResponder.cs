using System.Text.RegularExpressions;
using Bazam.DestupidifiedCollections;
using MargieBot.ExampleResponders.Models;
using MargieBot.Models;
using MargieBot.Responders;

namespace MargieBot.UI.Infrastructure.BotResponders
{
    public class BountyResponder : IResponder
    {
        private DestupidifiedList<string> _ActiveBounties = new DestupidifiedList<string>();
        private const string BOUNTY_START_REGEX = @"bounty[\s:]+(?<bountyText>[\s\S]+)";
        private const string BOUNTY_SUCCESS_REGEX = @"^<@(?<userId>[A-Z0-9]+)>\s+(wins|got it|gets it)$";
        private const string BOUNTY_CANCEL_REGEX = @"\bcancel\b\s*\bbounty$";

        public bool CanRespond(ResponseContext context)
        {
            return (
                (context.Message.ChatHub.Type != SlackChatHubType.DM && context.Message.MentionsBot && Regex.IsMatch(context.Message.Text, BOUNTY_START_REGEX, RegexOptions.IgnoreCase)) || (
                    _ActiveBounties.Contains(context.Message.User.ID) && (
                        Regex.IsMatch(context.Message.Text, BOUNTY_SUCCESS_REGEX, RegexOptions.IgnoreCase) ||
                        Regex.IsMatch(context.Message.Text, BOUNTY_CANCEL_REGEX, RegexOptions.IgnoreCase)
                    )
                )
            );
        }

        public BotMessage GetResponse(ResponseContext context)
        {
            Match bountyStartMatch = Regex.Match(context.Message.Text, BOUNTY_START_REGEX, RegexOptions.IgnoreCase);
            if(bountyStartMatch.Success) {
                _ActiveBounties.Add(context.Message.User.ID);
                return new BotMessage() {
                    Text = "It's bounty huntin' time! " + context.Message.User.FormattedUserID + " is givin' out a point for the best answer to: _" + bountyStartMatch.Groups["bountyText"].Value + "_"
                };
            }
            else if (Regex.IsMatch(context.Message.Text, BOUNTY_CANCEL_REGEX, RegexOptions.IgnoreCase)) {
                _ActiveBounties.Remove(context.Message.User.ID);
                return new BotMessage() {
                    Text = context.Message.User.FormattedUserID + " canceled his/her bounty. No points for that one, y'all."
                };
            }
            else {
                Match bountyEndMatch = Regex.Match(context.Message.Text, BOUNTY_SUCCESS_REGEX, RegexOptions.IgnoreCase);
                string winningUser = bountyEndMatch.Groups["userId"].Value;

                if (context.Message.User.ID == winningUser) {
                    return new BotMessage() {
                        Text = "Now, " + context.Message.User.FormattedUserID + " - you're not gettin' by me that easy. Tsk tsk."
                    };
                }
                else {
                    Scorebook scorebook = context.Get<Scorebook>();
                    scorebook.ScoreUser(winningUser, 1);
                    string formattedWinningUserId = "<@" + winningUser + ">";
                    _ActiveBounties.Remove(context.Message.User.ID);

                    return new BotMessage() {
                        Text = "Whoohoo! " + formattedWinningUserId + " just completed " + context.Message.User.FormattedUserID + "'s bounty for a point. " + formattedWinningUserId + ", your score is now " + scorebook.GetUserScore(winningUser) + "."
                    };
                }
            }
        }
    }
}