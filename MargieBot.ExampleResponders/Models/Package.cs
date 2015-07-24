using System.Text.RegularExpressions;

namespace MargieBot.ExampleResponders.Models
{
    public class Package
    {
        public string Description { get; set; }
        public string TrackingNumber { get; set; }
        public string UserID { get; set; }

        public PackageCarrier Carrier
        {
            get
            {
                if (Regex.IsMatch(TrackingNumber, "9400[0-9]{18}")) {
                    return PackageCarrier.USPS;
                }

                return PackageCarrier.UPS;
            }
        }
    }
}