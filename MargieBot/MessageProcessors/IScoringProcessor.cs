using MargieBot.Models;

namespace MargieBot.MessageProcessors
{
    public interface IScoringProcessor
    {
        bool IsScoringMessage(SlackMessage message);
        ScoreResult Score(SlackMessage message);
    }
}