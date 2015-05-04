namespace MargieBot.Models
{
    public class SlackMessage
    {
        public SlackChatHub ChatHub { get; set; }
        public bool MentionsBot { get; set; }
        public string RawData { get; set; }
        public string Text { get; set; }
        public string UserID { get; set; }

        public string FormattedUser
        {
            get {
                if (!string.IsNullOrEmpty(UserID)) {
                    return "<@" + UserID + ">";
                }
                return string.Empty;
            }
        }
    }
}