namespace MargieBot.Models
{
    public class SlackMessage
    {
        public string Channel { get; set; }
        public string RawData { get; set; }
        public string Text { get; set; }
        public string User { get; set; }

        public string FormattedUser
        {
            get {
                if (!string.IsNullOrEmpty(User)) {
                    return "<@" + User + ">";
                }
                return string.Empty;
            }
        }
    }
}