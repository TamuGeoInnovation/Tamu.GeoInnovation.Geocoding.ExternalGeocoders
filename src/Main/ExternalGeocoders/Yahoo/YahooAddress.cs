namespace USC.GISResearchLab.Geocoding.Core.ExternalGeocoders.Yahoo
{
    public class YahooAddress
    {
        public string Street { get; set; }
        public string City { get; set; }
        public string StateCode { get; set; }
        public string ZipCode { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string Precision { get; set; }
        //public string Warning { get; set; }
        public string ErrorMessage { get; set; }

        public YahooAddress(string street, string city, string stateCode, string zipCode, string latitude, string longitude, string precision, string warning, string errorMessage)
        {
            this.Street = street;
            this.City = city;
            this.StateCode = stateCode;
            this.ZipCode = zipCode;
            this.Latitude = latitude;
            this.Longitude = longitude;
            this.Precision = precision;
            //this.Warning = warning;
            this.ErrorMessage = errorMessage;
        }
    }
}
