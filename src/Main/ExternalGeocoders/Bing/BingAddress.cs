namespace USC.GISResearchLab.Geocoding.Core.ExternalGeocoders.Yahoo
{
    public class BingAddress
    {
        public string Street { get; set; }
        public string Type { get; set; }
        public string City { get; set; }
        public string StateCode { get; set; }
        public string ZipCode { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Precision { get; set; }
        public string MatchType { get; set; }
        public string Warning { get; set; }
        public string ErrorMessage { get; set; }
        public string Confidence { get; set; }

        public BingAddress()
        {
        }
    }
}
