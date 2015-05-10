using System;
using System.Text;
using System.Text.RegularExpressions;
using MargieBot.MessageProcessors;
using MargieBot.Models;
using MargieBot.UI.Infrastructure.Models;
using MargieBot.UI.Infrastructure.Models.DnD;

namespace MargieBot.UI.Infrastructure.BotResponseProcessors.DnDResponseProcessors
{
    public class RollResponseProcessor : IResponseProcessor
    {
        private const string DICE_REGEX = @"(?<NumberOfDice>a?[0-9])d(?<NumberOfSides>[1-9][0-9]*)";

        public bool CanRespond(ResponseContext context)
        {
            return context.Message.MentionsBot && Regex.IsMatch(context.Message.Text, @"\broll\b", RegexOptions.IgnoreCase) && Regex.IsMatch(context.Message.Text, DICE_REGEX, RegexOptions.IgnoreCase);
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
    }
}