using System.Linq;
using System.Xml.Linq;
using Bazam.NoobWebClient;

namespace MargieBot.ExampleResponders.Models
{
    public class UspsPackageStatusRequest
    {
        private const string USPS_API_URL = @"http://production.shippingapis.com/ShippingAPI.dll?API=TrackV2&XML=<?xml version=""1.0"" encoding=""UTF-8"" ?><TrackRequest USERID=""{0}""><TrackID ID=""{1}""></TrackID></TrackRequest>";

        public string ApiKey { get; set; }
        public string TrackingNumber { get; set; }

        public string Get()
        {
            string url = string.Format(USPS_API_URL, ApiKey, TrackingNumber);
            string packageStatusXml = new NoobWebClient().GetResponse(url, RequestMethod.Get).GetAwaiter().GetResult();
            XDocument doc = XDocument.Parse(packageStatusXml);
            XElement el = (from e in doc.Descendants("TrackSummary") select e).FirstOrDefault();

            return el?.Value;
        }
    }
}