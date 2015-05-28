using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Bazam.Modules.Enumerations;
using MargieBot.MessageProcessors;
using MargieBot.Models;
using MargieBot.UI.Infrastructure.Models.DnD;

namespace MargieBot.UI.Infrastructure.BotResponseProcessors.DnDResponseProcessors
{
    public class SavingThrowResponseProcessor : IResponseProcessor
    {
        /*
         * This process lets Margie make saving throws in D&D!
         * 
         * It allows two syntaxes. The "power user" syntax tells her which attribute to roll and supplies the spell DC of the caster in the same statement, like this:
         * "margie, make a con save dc13". That one is really pretty easy, because it's just like a normal processor - if the input matches the pattern, roll and respond. Ezpz.
         * 
         * Howevah, I thought it'd be fun if you could "forget" to tell Margie the spell DC of the caster in your first statement to her and let her ask for it. She can store up to one
         * pending saving throw for each user that asks for one, and if the user says a number, she assumes that to be the spell DC and completes the roll. So the interaction
         * looks like this:
         * 
         * "margie, make a con saving throw"
         * "Sure, hun! I got all kindsa constitution. It says here I need your spell... dee... cee... I don't know what that is. What's yours?"
         * "14"
         * "Okay... here we go... Nuts! I only got 12. Did somethin' horrible just happen?"
        */
        private const string DC_PROVIDED_REGEX = @"dc\s*(?<dcValue>\d+)\b";
        private const string DC_VALUE_REGEX = @"\b\d+\b";
        private const string THROW_REQUEST_REGEX = @"(?<attribute>CHA|Charisma|CON|Constitution|DEX|Dexterity|INT|Intelligence|STR|Strength|WIS|Wisdom)\s+(save|saving\s+throw)";
        private Dictionary<CharacterAttribute, string> ATTRIBUTE_COMMENTARY = new Dictionary<CharacterAttribute, string>() {
            { CharacterAttribute.CHA, "Why, of course. You want some of that old-fashioned Southern charm?" },
            { CharacterAttribute.CON, "You know it! I'm a hearty gal, especially in Dungeons and Draggin's!" },
            { CharacterAttribute.DEX, "Sure thang. It's hard to be dextrous without hands in real life, but my character'll give 'er a try." },
            { CharacterAttribute.INT, "You betcha. I'm smart, y'all. You know them SATs? I know somebody that _took_ 'em. That's right." },
            { CharacterAttribute.STR, "RAWR! Want to test these muscles? Bring it on." },
            { CharacterAttribute.WIS, "Oh yeah. I got lots of wisdom. Y'all heard of that Sun Zoo? I've never been, but I heard it's nice." },
        };

        private Dictionary<string, CharacterAttribute> _PendingThrows = new Dictionary<string, CharacterAttribute>();

        public bool CanRespond(ResponseContext context)
        {
            return
                (context.Message.MentionsBot && Regex.IsMatch(context.Message.Text, THROW_REQUEST_REGEX, RegexOptions.IgnoreCase)) ||
                (_PendingThrows.Keys.Contains(context.Message.User.ID) && Regex.IsMatch(context.Message.Text, DC_VALUE_REGEX));
        }

        public BotMessage GetResponse(ResponseContext context)
        {
            StringBuilder messageText = new StringBuilder();

            // first check if this is resolving a pending throw
            if (!Regex.IsMatch(context.Message.Text, THROW_REQUEST_REGEX, RegexOptions.IgnoreCase) && _PendingThrows.Keys.Contains(context.Message.User.ID)) {
                Match dcValueMatch = Regex.Match(context.Message.Text, DC_VALUE_REGEX);
                int dcValue = Convert.ToInt32(dcValueMatch.Value);

                messageText.Append("Okay, got it. Here we go, y'all!");
                messageText.Append(ComposeRollResponse(_PendingThrows[context.Message.User.ID], dcValue));

                // remove the pending throw since we're resolving it here
                _PendingThrows.Remove(context.Message.User.ID);
            }
            else {
                Match throwRequestMatch = Regex.Match(context.Message.Text, THROW_REQUEST_REGEX, RegexOptions.IgnoreCase);

                if (throwRequestMatch.Success) {
                    string attrData = throwRequestMatch.Groups["attribute"].Value;
                    if (attrData.Length > 3) {
                        attrData = attrData.Substring(0, 3);
                    }
                    CharacterAttribute attr = EnuMaster.Parse<CharacterAttribute>(attrData, true);

                    if (Regex.IsMatch(context.Message.Text, DC_PROVIDED_REGEX, RegexOptions.IgnoreCase)) {
                        messageText.Append("Alrighty! One ");
                        messageText.Append(attr.ToString());
                        messageText.Append(" roll comin' up!");

                        string dcData = Regex.Match(context.Message.Text, DC_PROVIDED_REGEX, RegexOptions.Compiled).Groups["dcValue"].Value;
                        int dc = Convert.ToInt32(dcData);

                        messageText.Append(ComposeRollResponse(attr, dc));
                    }
                    else {
                        messageText.Append(ATTRIBUTE_COMMENTARY[attr]);
                        messageText.Append(" It says here I need your spell... dee... cee. I don't really know what that means, do you? What's yours?");
                        _PendingThrows.Add(context.Message.User.ID, attr);
                    }
                }
            }

            return new BotMessage() {
                Text = messageText.ToString()
            };
        }

        private string ComposeRollResponse(CharacterAttribute attr, int dc)
        {
            Character margiesChar = new Character();
            Die die = new Die() { NumberOfSides = 20 };
            int attrModifier = margiesChar.GetAttributeBonus(attr);
            int proficiencyBonus = margiesChar.GetAttrProficiency(attr);
            int baseRoll = die.Roll();
            int finalRoll = baseRoll + proficiencyBonus + attrModifier;

            StringBuilder builder = new StringBuilder();

            builder.Append(" ```");
            builder.Append(baseRoll);
            builder.Append(" (base roll) + ");
            builder.Append(attrModifier);
            builder.Append(" (attribute bonus)");

            if (proficiencyBonus > 0) {
                builder.Append(" + ");
                builder.Append(proficiencyBonus);
                builder.Append(" (proficiency bonus)");
            }

            builder.Append(" = ");
            builder.Append(baseRoll + attrModifier + proficiencyBonus);
            builder.Append("``` ");

            if (finalRoll > dc) {
                builder.Append("Hah! Gotcha. What'd I dodge??");
            }
            else if (finalRoll >= (dc-1)) {
                builder.Append("Awwww, man! So close. I'm 'bout to regret it, ain't I?");
            }
            else {
                builder.Append("Nerts. That ain't good. Did somethin' horrible just happen?");
            }
            
            return builder.ToString();
        }
    }
}