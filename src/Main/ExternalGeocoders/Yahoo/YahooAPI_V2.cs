using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Xml.Serialization;
using USC.GISResearchLab.Common.Utils.Encoding;

namespace USC.GISResearchLab.Geocoding.Core.ExternalGeocoders.Yahoo
{

    public class YahooAPI_V2
    {

        // from http://stackoverflow.com/questions/4580432/having-a-problem-deserializing-types-returned-by-yahoo-geocoding-service
        [Serializable]
        [XmlRoot(ElementName = "ResultSet")]
        public class PlaceFinderResultSet
        {

            public PlaceFinderResultSet() { Results = new List<Result>(); }

            [XmlElement("Error")]
            public int Error { get; set; }

            [XmlElement("ErrorMessage")]
            public string ErrorMessage { get; set; }

            [XmlElement("Locale")]
            public string Locale { get; set; }

            [XmlElement("Quality")]
            public int Quality { get; set; }

            [XmlElement("Found")]
            public int Found { get; set; }

            [XmlElement("Result")]
            public List<Result> Results { get; set; }
        }

        //[XmlRoot(ElementName = "ResultSet", Namespace = "")]
        //public class GeoCoordinates
        //{
        //    [XmlElement(ElementName = "Result")]
        //    public Result result { get; set; }
        //}

        [XmlRoot(ElementName = "")]
        public class Result
        {
            [XmlElement(ElementName = "longitude")]
            public string Longitude { get; set; }
            [XmlElement(ElementName = "latitude")]
            public string Latitude { get; set; }

            [XmlElement(ElementName = "line1")]
            public string Line1 { get; set; }

            [XmlElement(ElementName = "quality")]
            public string Quality { get; set; }
            [XmlElement(ElementName = "line2")]
            public string Line2 { get; set; }
            [XmlElement(ElementName = "line3")]
            public string Line3 { get; set; }
            [XmlElement(ElementName = "line4")]
            public string Line4 { get; set; }
            [XmlElement(ElementName = "house")]
            public string House { get; set; }
            [XmlElement(ElementName = "street")]
            public string Street { get; set; }
            [XmlElement(ElementName = "unittype")]
            public string UnitType { get; set; }
            [XmlElement(ElementName = "unit")]
            public string Unit { get; set; }
            [XmlElement(ElementName = "postal")]
            public string Postal { get; set; }
            [XmlElement(ElementName = "neighborhood")]
            public string Neighborhood { get; set; }
            [XmlElement(ElementName = "city")]
            public string city { get; set; }
            [XmlElement(ElementName = "county")]
            public string county { get; set; }
            [XmlElement(ElementName = "statecode")]
            public string statecode { get; set; }
        }

        public static List<YahooAddress> Geocode(string street, string city, string stateCode, string zipCode, string apiUrl)
        {

            List<YahooAddress> ret = new List<YahooAddress>();
            if (String.IsNullOrEmpty(apiUrl))
            {
                apiUrl = "http://where.yahooapis.com/geocode?appid=HpylNHXV34HfGas3wvXQdChAhEXofLAxZ7aR0JQ.fB.eX0u72HSCHd0OHl12wjuo";
            }

            string latitude = "";
            string longitude = "";
            string precison = "";
            string warning = "";
            string errorMessage = "";

            // Build URL request to be sent to Yahoo!
            string url = "";

            string line1 = "";
            string line2 = "";

            if (street.Length > 0)
            {
                line1 += "&line1=" + WebEncodingUtils.URLEncode(street);
            }

            if (city.Length > 0 || stateCode.Length > 0 || zipCode.Length > 0)
            {
                line2 += "&line2=";

                if (city.Length > 0)
                {
                    line2 += WebEncodingUtils.URLEncode(city);
                }

                if (stateCode.Length == 2)
                {
                    if (!String.IsNullOrEmpty(line1))
                    {
                        line2 += (", ");
                    }

                    line2 += WebEncodingUtils.URLEncode(stateCode);
                }

                if (zipCode.Length >= 5)
                {
                    if (!String.IsNullOrEmpty(line2))
                    {
                        line2 += (" ");
                    }
                    else
                    {
                        if (!String.IsNullOrEmpty(line1))
                        {
                            line2 += (", ");
                        }
                    }

                    line2 += WebEncodingUtils.URLEncode(zipCode);
                }
            }

            url = apiUrl;

            if (!String.IsNullOrEmpty(line1))
            {
                url += line1;
            }

            if (!String.IsNullOrEmpty(line2))
            {
                url += line2;
            }

            url = url.Replace(" ", "+");  // Yahoo example shows + sign instead of spaces.

            HttpWebRequest request = null;
            // Read Returned XML file from YAHOO!
            try
            {

                //create a new httprequest
                request = (HttpWebRequest)HttpWebRequest.Create(url);
                request.Timeout = 2000;

                //Get the response
                var response = request.GetResponse();

                //Serializing Response XML to C# Class
                XmlSerializer sr = new XmlSerializer(typeof(PlaceFinderResultSet));
                var placeFinderResultSet = sr.Deserialize(response.GetResponseStream()) as PlaceFinderResultSet;

                foreach (Result result in placeFinderResultSet.Results)
                {

                    street = result.Line1;
                    city = result.city;
                    stateCode = result.statecode;
                    zipCode = result.Postal;
                    latitude = result.Latitude;
                    longitude = result.Longitude;
                    precison = result.Quality;

                    YahooAddress address = new YahooAddress(street, city, stateCode, zipCode, latitude, longitude, precison, warning, errorMessage);
                    ret.Add(address);
                }


            }
            catch (Exception e)
            {
                Serilog.Log.Error(e, MethodBase.GetCurrentMethod().GetType().Name + " " + MethodBase.GetCurrentMethod().Name + ": " + e.Message);
                YahooAddress address = new YahooAddress(street, city, stateCode, zipCode, latitude, longitude, precison, warning, errorMessage);
                ret.Add(address);
            }

            return ret;
        }
    }
}
