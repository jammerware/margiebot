namespace MargieBot.Infrastructure.Models
{
    public class UserContext
    {
        public bool HasScoredPreviously { get; set; }
        public int Score { get; set; }
    }
}