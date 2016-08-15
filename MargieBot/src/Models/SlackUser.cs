namespace MargieBot
{
    public class SlackUser
    {
        public string ID { get; set; }

        public string FormattedUserID
        {
            get
            {
                if (!string.IsNullOrEmpty(ID)) {
                    return "<@" + ID + ">";
                }
                return string.Empty;
            }
        }

        public bool IsSlackbot
        {
            get { return ID == "USLACKBOT"; }
        }
    }
}