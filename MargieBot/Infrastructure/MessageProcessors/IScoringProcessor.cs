using MargieBot.Infrastructure.Models;

namespace MargieBot.Infrastructure.MessageProcessors
{
    public interface IScoringProcessor
    {
        bool IsScoringMessage(SlackMessage message);
        ScoreResult Score(SlackMessage message);
    }
}