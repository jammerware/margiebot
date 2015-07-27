using System;
using System.Text;
using System.Text.RegularExpressions;
using MargieBot.ExampleResponders.Responders;
using MargieBot.Models;
using MargieBot.Responders;
using MargieBot.UI.Infrastructure.Models.DnD;

namespace MargieBot.UI.Infrastructure.BotResponders.DnDResponders
{
    public class RollResponder : IResponder, IDescribable
    {
        private const string DICE_REGEX = @"(?<NumberOfDice>[0-9]+)d(?<NumberOfSides>[1-9][0-9]*)";

        public bool CanRespond(ResponseContext context)
        {
            return (context.Message.MentionsBot || context.Message.ChatHub.Type == SlackChatHubType.DM) && Regex.IsMatch(context.Message.Text, @"\broll\b", RegexOptions.IgnoreCase) && Regex.IsMatch(context.Message.Text, DICE_REGEX, RegexOptions.IgnoreCase);
        }

        public BotMessage GetResponse(ResponseContext context)
        {
            StringBuilder builder = new StringBuilder("Alright. Wish me luck, y'all! Lessee here...\n\n`");
            int runningTotal = 0;
            bool conversionFailed = false;

            foreach (Match match in Regex.Matches(context.Message.Text, DICE_REGEX)) {
                int numberOfDice = 0;
                try {
                    numberOfDice = Convert.ToInt32(match.Groups["NumberOfDice"].Value);
                }
                catch (Exception) {
                    conversionFailed = true;
                    break;
                }

                // apparently my coworkers are literally incapable of not breaking things for fun. you'd think a bunch of developers could... just... nevermind.
                // ...
                // no, you know what? i understand that you really CAN idiot-proof everything, and that that's highly necessary in an actual production application,
                // but this is a BOT in slack in a room full of developers. REALLY? you're mad she broke because you asked her to roll 9000 9000-sided dice? I'M SORRY.
                //
                // GOD.
                if (numberOfDice > 100) {
                    conversionFailed = true;
                }

                Die die = new Die();
                try {
                    die.NumberOfSides = Convert.ToInt32(match.Groups["NumberOfSides"].Value);
                }
                catch(Exception) {
                    conversionFailed = true;
                    break;
                }

                if (numberOfDice > 1) {
                    builder.Append("(");
                }

                for (int i = 0; i < numberOfDice; i++) {
                    if (i > 0) {
                        builder.Append(" + ");
                    }

                    int thisRoll = die.Roll();
                    runningTotal += thisRoll;

                    builder.Append(thisRoll.ToString());
                }

                if (numberOfDice > 1) {
                    builder.Append(")");
                }
            }

            builder.Append("`\n\n");
            builder.Append("Y'all! I got a " + runningTotal.ToString() + ". How'd I do???");

            if (!conversionFailed && builder.Length > 0) {
                return new BotMessage() { Text = builder.ToString() };
            }
            else {
                return new BotMessage() { Text = "Are y'all funnin' with me again?" };
            }
        }

        #region IDescribable
        public string Description
        {
            get { return "roll some dice for you. Try asking me to roll 4d6!"; }
        }
        #endregion
    }
}