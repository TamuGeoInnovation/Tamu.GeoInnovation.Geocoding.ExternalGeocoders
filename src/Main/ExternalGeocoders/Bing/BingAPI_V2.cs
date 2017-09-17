//using USC.GISResearchLab.Geocoding.Core.BingGeocodeService;
using System;
using System.Collections.Generic;
using Tamu.GeoInnovation.Geocoding.Core.BingGeocodeService_V2;
using USC.GISResearchLab.Common.Utils.Strings;
using USC.GISResearchLab.Geocoding.Core.ExternalGeocoders.Yahoo;
//using Tamu.GeoInnovation.Geocoding.Core.BingGeocodeService_V2;

namespace USC.GISResearchLab.Geocoding.Core.ExternalGeocoders.Bing
{

    public class BingAPI_V2
    {

        //static string AppID = "F81AE211F7FD1ADA1BAE43D51613949113C41FC3";

        //public static List<BingAddress> Geocode(string street, string city, string stateCode, string zipCode, string apiUrl)
        //{

        //    List<BingAddress> ret = new List<BingAddress>();
        //    if (String.IsNullOrEmpty(apiUrl))
        //    {
        //        //apiUrl = "http://local.yahooapis.com/MapsService/V1/geocode?appid=HpylNHXV34HfGas3wvXQdChAhEXofLAxZ7aR0JQ.fB.eX0u72HSCHd0OHl12wjuo";
        //        apiUrl = "http://dev.virtualearth.net/services/v1/geocodeservice/geocodeservice.asmx/Geocode?query=&count=&countryRegion=&culture=&curLocAccuracy=&currentLocation=&district=&entityTypes=&landmark=&mapBounds=&rankBy=";
        //    }

        //    string latitude = "";
        //    string longitude = "";
        //    string precison = "";
        //    string warning = "";
        //    string errorMessage = "";

        //    // Build URL request to be sent to Yahoo!
        //    string url = "";

        //    url += "&addressLine=" + street;


        //    url += "&postalTown=" + city;


        //    url += "&locality=" + city;


        //    url += "&adminDistrict=" + stateCode;


        //    url += "&postalCode=" + zipCode;


        //    url = apiUrl + url;
        //    url = url.Replace(" ", "+");


        //    string resultStreet = "";
        //    string resultType = "";
        //    string resultCity = "";
        //    string resultState = "";
        //    string resultZip = "";

        //    try
        //    {
        //        XmlReader reader = new XmlTextReader(url);
        //        XmlDocument xDoc = new XmlDocument();
        //        xDoc.Load(reader);
        //        /*
        //           There is no default namespace being used, meaning that all non-prefixed (or embedded) elements DO NOT belong to ANY namespace. 
        //           This allows you to execute XPath statements without prefixes. 
        //           XPath queries does not support 'default' namespaces. 
        //           You have to adjust your XPath query to include that namespace. 
        //       */
        //        XmlNamespaceManager xMngr = new XmlNamespaceManager(xDoc.NameTable);
        //        xMngr.AddNamespace("bing", "http://dev.virtualearth.net/");


        //        XmlNode response = xDoc.FirstChild;// ("//GeocodingResult");

        //        XmlNodeList resultList = xDoc.SelectNodes("//bing:GeocodingResult", xMngr);



        //        foreach (XmlNode node in resultList)
        //        {

        //            resultStreet = ((XmlNode)node.SelectSingleNode("./bing:Name", xMngr)).InnerText;
        //            resultStreet = ((XmlNode)node.SelectSingleNode("./bing:Type", xMngr)).InnerText;


        //            XmlNode bestResultNode = node.SelectSingleNode("./bing:Type", xMngr);
        //            resultCity = ((XmlNode)node.SelectSingleNode("./bing:Type", xMngr)).InnerText;

        //            XmlNode xCity;
        //            xCity = node.SelectSingleNode("//bing:City", xMngr);
        //            city = xCity.InnerText;

        //            XmlNode xStateCode;
        //            xStateCode = node.SelectSingleNode("//bing:State", xMngr);
        //            stateCode = xStateCode.InnerText;

        //            XmlNode xZipCode;
        //            xZipCode = node.SelectSingleNode("//bing:Zip", xMngr);
        //            zipCode = xZipCode.InnerText;

        //            XmlNode xLatitude;
        //            xLatitude = node.SelectSingleNode("//bing:Latitude", xMngr);
        //            latitude = xLatitude.InnerText;
        //            XmlNode xLongitude;
        //            xLongitude = node.SelectSingleNode("//bing:Longitude", xMngr);
        //            longitude = xLongitude.InnerText;
        //            XmlNode xPrecision;
        //            xPrecision = node.SelectSingleNode("//bing:Result/@precision", xMngr);
        //            precison = xPrecision.InnerText;

        //            XmlNode xWarning;
        //            xWarning = xDoc.SelectSingleNode("//bing:Result/@warning", xMngr);
        //            warning = xWarning.InnerText;

        //            BingAddress address = new BingAddress(resultStreet, city, stateCode, zipCode, latitude, longitude, resultType, precison, warning, errorMessage);

        //            ret.Add(address);
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        errorMessage = e.Message;
        //        BingAddress address = new BingAddress(resultStreet, city, stateCode, zipCode, latitude, longitude, resultType, precison, warning, errorMessage);
        //        ret.Add(address);
        //    }

        //    return ret;
        //}


        public static List<BingAddress> Geocode(string street, string city, string stateCode, string zipCode)
        {

            List<BingAddress> ret = new List<BingAddress>();


            string key = "AucI0EYs0YQZcfMnu30Ntb8GNTJDDpNJ32jKYh1Ruw1GpvmQnf65s82G0LnMROLG";
            GeocodeRequest geocodeRequest = new GeocodeRequest();

            // Set the credentials using a valid Bing Maps key
            
            geocodeRequest.Credentials = new Credentials();
            geocodeRequest.Credentials.ApplicationId = key;

            // Set the location of the requested image
            geocodeRequest.Address = new Address();
            geocodeRequest.Address.AddressLine = street;
            geocodeRequest.Address.PostalTown = city;
            geocodeRequest.Address.PostalCode = zipCode;
            geocodeRequest.Address.AdminDistrict = stateCode;

            // Set the full address query
            //geocodeRequest.Query = address;


            // Set the options to only return high confidence results 
            ConfidenceFilter[] filters = new ConfidenceFilter[1];
            filters[0] = new ConfidenceFilter();
            filters[0].MinimumConfidence = Confidence.High;

            // Add the filters to the options
            GeocodeOptions geocodeOptions = new GeocodeOptions();
            geocodeOptions.Filters = filters;
            geocodeRequest.Options = geocodeOptions;



            // Make the geocode request
            Tamu.GeoInnovation.Geocoding.Core.BingGeocodeService_V2.GeocodeServiceClient geocodeService = new Tamu.GeoInnovation.Geocoding.Core.BingGeocodeService_V2.GeocodeServiceClient("BasicHttpBinding_IGeocodeService");
            GeocodeResponse geocodeResponse = geocodeService.Geocode(geocodeRequest);



            foreach (GeocodeResult geocodeResult in geocodeResponse.Results)
            {
                int i = 0;
                foreach (GeocodeLocation location in geocodeResult.Locations)
                {

                    BingAddress address = new BingAddress();
                    address.Street = geocodeResult.Address.AddressLine;
                    address.City = geocodeResult.Address.Locality;
                    address.StateCode = geocodeResult.Address.AdminDistrict;
                    address.ZipCode = geocodeResult.Address.PostalCode;

                    address.Latitude = location.Latitude;
                    address.Longitude = location.Longitude;
                    address.Confidence = geocodeResult.Confidence.ToString();

                    bool isUphierarchy = false;

                    if (!String.IsNullOrEmpty(street))
                    {
                        address.Precision = location.CalculationMethod;
                    }
                    else
                    {
                        isUphierarchy = true;
                        if (!String.IsNullOrEmpty(zipCode))
                        {
                            if (string.Compare(location.CalculationMethod, "Rooftop", true) == 0)
                            {
                                address.Precision = "Interpolation";
                            }
                            else
                            {
                                address.Precision = location.CalculationMethod;
                            }
                        }
                        else if (!String.IsNullOrEmpty(zipCode))
                        {
                            if (string.Compare(location.CalculationMethod, "Rooftop", true) == 0)
                            {
                                address.Precision = "Interpolation";
                            }
                            else
                            {
                                address.Precision = location.CalculationMethod;
                            }
                        }
                        else if (!String.IsNullOrEmpty(stateCode))
                        {
                            if (string.Compare(location.CalculationMethod, "Rooftop", true) == 0)
                            {
                                address.Precision = "Interpolation";
                            }
                            else
                            {
                                address.Precision = location.CalculationMethod;
                            }
                        }
                    }



                    address.MatchType = StringUtils.ConcatArrayWithCharBetween(geocodeResult.MatchCodes, ";");

                    if (isUphierarchy)
                    {
                        address.MatchType += ";uphierarchy";
                    }

                    ret.Add(address);

                    i++;
                }
            }

            return ret;
        }
    }
}
