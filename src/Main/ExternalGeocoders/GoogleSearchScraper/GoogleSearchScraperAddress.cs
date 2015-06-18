namespace USC.GISResearchLab.Geocoding.Core.ExternalGeocoders.GoogleSearchScraper
{
    public class GoogleSearchScraperAddress
    {
        public string Name { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string StateCode { get; set; }
        public string ZipCode { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string Precision { get; set; }
        public string Phone { get; set; }
        public string Warning { get; set; }
        public string ErrorMessage { get; set; }

        public GoogleSearchScraperAddress()
        {
        }

        public GoogleSearchScraperAddress(string name, string street, string city, string stateCode, string zipCode, string phone, string latitude, string longitude, string precision, string warning, string errorMessage)
        {
            this.Name = name;
            this.Street = street;
            this.City = city;
            this.StateCode = stateCode;
            this.ZipCode = zipCode;
            this.Latitude = latitude;
            this.Longitude = longitude;
            this.Precision = precision;
            this.Phone = phone;
            this.Warning = warning;
            this.ErrorMessage = errorMessage;
        }
    }
}
