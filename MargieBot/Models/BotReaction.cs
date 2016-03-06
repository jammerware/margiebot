namespace MargieBot.Models
{
    public class BotReaction : BotResponse
    {
        /// <summary>
        /// Reaction (emoji) name
        /// </summary>
        public string Name { get; set; }

        public BotReactionType ReactionType { get; set; }

        /// <summary>
        /// Source Id
        /// File Id, File Comment Id or Channel Id, depending on <see cref="BotReactionType">ReactionType</see>.
        /// </summary>
        public string SourceId { get; set; }

        /// <summary>
        /// Timestamp of the message to add reaction to (if <see cref="BotReactionType">ReactionType</see> is Channel.
        /// </summary>
        public string TimeStamp { get; set; }
    }
}