using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using MargieBot.ExampleResponders.Models;
using MargieBot.Models;
using MargieBot.Responders;
using Newtonsoft.Json;

namespace MargieBot.ExampleResponders.Responders
{
    public class PackageTrackerResponder : IResponder, IDescribable
    {
        private const string FILE_PATH_BASE = "package-tracker-{0}.json";
        private const string NEW_TRACK_REQUEST_REGEX = @"\btrack\s+my\s((?<description>[\s\S]+)\sat\s)?(?<number>[a-zA-Z0-9]+)";
        private const string STOP_TRACK_REQUEST_REGEX = @"\b(don't\s+track|stop\s+tracking)\s+(?<number>[a-zA-Z0-9]+)\b";
        private const string UPDATE_REQUEST_REGEX = @"\bwhere('s)?\s+my\s+stuff\b";
        private const string USPS_NUMBER_REGEX = @"9400[0-9]{18}";
        
        private List<Package> _ActivePackages = new List<Package>();
        private string FilePath
        {
            get { return string.Format(FILE_PATH_BASE, TeamID); }
        }

        public string UspsApiKey { get; set; }
        public string TeamID { get; set; }

        public PackageTrackerResponder(string uspsApiKey)
        {
            this.UspsApiKey = uspsApiKey;
        }

        #region IResponder
        public bool CanRespond(ResponseContext context)
        {
            if (this.TeamID != context.TeamID) { this.TeamID = context.TeamID; }
            if (File.Exists(FilePath)) {
                _ActivePackages = JsonConvert.DeserializeObject<List<Package>>(File.ReadAllText(FilePath));
            }

            if (context.Message.ChatHub.Type == SlackChatHubType.DM || context.Message.MentionsBot) {
                return 
                    Regex.IsMatch(context.Message.Text, NEW_TRACK_REQUEST_REGEX, RegexOptions.IgnoreCase) ||
                    Regex.IsMatch(context.Message.Text, STOP_TRACK_REQUEST_REGEX, RegexOptions.IgnoreCase) ||
                    Regex.IsMatch(context.Message.Text, UPDATE_REQUEST_REGEX, RegexOptions.IgnoreCase);
            }

            return false;
        }

        public BotMessage GetResponse(ResponseContext context)
        {
            Match newTrackMatch = Regex.Match(context.Message.Text, NEW_TRACK_REQUEST_REGEX, RegexOptions.IgnoreCase);
            Match stopTrackMatch = Regex.Match(context.Message.Text, STOP_TRACK_REQUEST_REGEX, RegexOptions.IgnoreCase);

            if (newTrackMatch.Success) {
                Package package = new Package() {
                    TrackingNumber = newTrackMatch.Groups["number"].Value,
                    UserID = context.Message.User.ID
                };

                if (newTrackMatch.Groups["description"].Success) {
                    package.Description = newTrackMatch.Groups["description"].Value;
                }

                // check on the package now
                UspsPackageStatusRequest request = new UspsPackageStatusRequest() { ApiKey = UspsApiKey, TrackingNumber = package.TrackingNumber };
                string status = request.Get();

                if (status != null) {
                    _ActivePackages.Add(package);
                    Save();

                    StringBuilder messageBuilder = new StringBuilder();
                    messageBuilder.Append("You got it, ");
                    messageBuilder.Append(context.Message.User.FormattedUserID);
                    messageBuilder.Append("! I got my eye on your ");

                    if (package.Description != null) {
                        messageBuilder.Append(package.Description);
                    }
                    else {
                        messageBuilder.Append("package");
                    }

                    messageBuilder.Append(". Right now it's here: ");
                    messageBuilder.AppendLine();
                    messageBuilder.Append(">");
                    messageBuilder.Append(status);

                    return new BotMessage() {
                        Text = messageBuilder.ToString()
                    };

                }
                else {
                    return new BotMessage() {
                        Text = "I couldn't find a package for that tracking number :( Sorry, " + context.Message.User.FormattedUserID + "!"
                    };
                }
            }
            else if (stopTrackMatch.Success) {
                string trackingNumber = stopTrackMatch.Groups["number"].Value;
                Package package = _ActivePackages.Where(p => p.UserID == context.Message.User.ID && p.TrackingNumber == trackingNumber).FirstOrDefault();

                if(package != null) {
                    _ActivePackages.Remove(package);
                    Save();

                    return new BotMessage() { Text = "I'll stop trackin' your " + package.Description + ". Hope it made it alright!" };
                }
                else {
                    return new BotMessage() { Text = "Hrrmmmm. I'm not trackin' a package with that number for you, " + context.Message.User.FormattedUserID + ". You sure?" };
                }

            }
            else if (Regex.IsMatch(context.Message.Text, UPDATE_REQUEST_REGEX, RegexOptions.IgnoreCase)) {
                IList<Package> packages = _ActivePackages.Where(p => p.UserID == context.Message.User.ID).ToList();

                if (packages.Count == 0) {
                    return new BotMessage() {
                        Text = @"I'm not trackin' any packages for ya right now, " + context.Message.User.FormattedUserID + @". Tell me to by sayin' somethin' like ""Hey Margie, track my bean skillet at <some trackin' number>."""
                    };
                }
                else {
                    StringBuilder builder = new StringBuilder();
                    builder.Append("I'm watchin' ");
                    builder.Append(packages.Count.ToString());
                    builder.Append(" package");
                    builder.Append(packages.Count == 1 ? string.Empty : "s");
                    builder.Append(" for ya, ");
                    builder.Append(context.Message.User.FormattedUserID);
                    builder.Append(". Here's the skinny: ");
                    builder.AppendLine();
                    builder.AppendLine();

                    foreach (Package package in packages) {
                        builder.AppendLine("*" + package.Description + "* (" + package.TrackingNumber + ")");
                        builder.Append(">");

                        UspsPackageStatusRequest request = new UspsPackageStatusRequest() { ApiKey = UspsApiKey, TrackingNumber = package.TrackingNumber };
                        string status = request.Get();
                        if (status != null) { builder.Append(status); }
                        else { builder.Append("Couldn't track this one down... sorry :("); }

                        builder.AppendLine();
                    }

                    return new BotMessage() {
                        Text = builder.ToString()
                    };
                }
            }

            return new BotMessage() {
                Text = "Oops. Trackin' stuff is hard for me, y'all. Sorry. Let me get back to ya."
            };
        }
        #endregion

        private void Save()
        {
            File.WriteAllText(FilePath, JsonConvert.SerializeObject(_ActivePackages));
        }

        #region IDescribable
        public string Description
        {
            get { return @"track USPS packages for ya. Tell me to watch one by sayin' something like ""margie, track my (whatever it is) at (the tracking number, like 9400111899562265395591). You can find out where everything by asking me ""where's my stuff?"", and you can tell me to stop tracking something by just saying ""stop tracking (the tracking number)"""; }
        }
        #endregion
    }
}