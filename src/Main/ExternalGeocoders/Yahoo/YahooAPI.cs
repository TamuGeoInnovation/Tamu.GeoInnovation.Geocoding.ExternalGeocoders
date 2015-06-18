using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;
using USC.GISResearchLab.Common.Utils.Encoding;

namespace USC.GISResearchLab.Geocoding.Core.ExternalGeocoders.Yahoo
{

    public class YahooAPI
    {

        public static List<YahooAddress> Geocode(string street, string city, string stateCode, string zipCode, string apiUrl)
        {

            List<YahooAddress> ret = new List<YahooAddress>();
            if (String.IsNullOrEmpty(apiUrl))
            {
                apiUrl = "http://local.yahooapis.com/MapsService/V1/geocode?appid=HpylNHXV34HfGas3wvXQdChAhEXofLAxZ7aR0JQ.fB.eX0u72HSCHd0OHl12wjuo";
            }

            string latitude = "";
            string longitude = "";
            string precison = "";
            string warning = "";
            string errorMessage = "";

            // Build URL request to be sent to Yahoo!
            string url = "";
            if (street.Length > 0)
            {
                url += "&street=" + WebEncodingUtils.URLEncode(street);
            }
            if (city.Length > 0)
            {
                url += "&city=" + WebEncodingUtils.URLEncode(city);
            }
            if (stateCode.Length == 2)
            {
                url += "&state=" + WebEncodingUtils.URLEncode(stateCode);
            }
            if (zipCode.Length >= 5)
            {
                url += "&zip=" + WebEncodingUtils.URLEncode(zipCode);
            }
            url = apiUrl + url;
            url = url.Replace(" ", "+");  // Yahoo example shows + sign instead of spaces.

            // Read Returned XML file from YAHOO!
            try
            {
                XmlReader reader = new XmlTextReader(url);
                XmlDocument xDoc = new XmlDocument();
                xDoc.Load(reader);
                /*
                   There is no default namespace being used, meaning that all non-prefixed (or embedded) elements DO NOT belong to ANY namespace. 
                   This allows you to execute XPath statements without prefixes. 
                   XPath queries does not support 'default' namespaces. 
                   You have to adjust your XPath query to include that namespace. 
               */
                XmlNamespaceManager xMngr = new XmlNamespaceManager(xDoc.NameTable);
                xMngr.AddNamespace("yahoo", "urn:yahoo:maps");


                XmlNodeList resultList = xDoc.SelectNodes("//yahoo:Result", xMngr);

                foreach (XmlNode node in resultList)
                {
                    try  /*  STREET */
                    {
                        XmlNode xStreet;
                        xStreet = node.SelectSingleNode("./yahoo:Address", xMngr);
                        street = xStreet.InnerText;
                    }
                    catch
                    {
                        street = "";
                    }

                    try /* CITY */
                    {
                        XmlNode xCity;
                        xCity = node.SelectSingleNode("./yahoo:City", xMngr);
                        city = xCity.InnerText;
                    }
                    catch
                    {
                        city = "";
                    }

                    try /* STATE */
                    {
                        XmlNode xStateCode;
                        xStateCode = node.SelectSingleNode("./yahoo:State", xMngr);
                        stateCode = xStateCode.InnerText;
                    }
                    catch
                    {
                        stateCode = "";
                    }

                    try /* ZIPCODE */
                    {
                        XmlNode xZipCode;
                        xZipCode = node.SelectSingleNode("./yahoo:Zip", xMngr);
                        zipCode = xZipCode.InnerText;
                    }
                    catch
                    {
                        zipCode = "";
                    }

                    try /* LATITUDE , LONGITUDE, PRECISION */
                    {
                        XmlNode xLatitude;
                        xLatitude = node.SelectSingleNode("./yahoo:Latitude", xMngr);
                        latitude = xLatitude.InnerText;
                        XmlNode xLongitude;
                        xLongitude = node.SelectSingleNode("./yahoo:Longitude", xMngr);
                        longitude = xLongitude.InnerText;
                        XmlNode xPrecision;
                        xPrecision = node.Attributes.GetNamedItem("precision");
                        precison = xPrecision.InnerText;
                    }
                    catch
                    {
                        latitude = "";
                        longitude = "";
                        precison = "";
                    }

                    try  // warning is only returned if address unclear
                    {
                        XmlNode xWarning;
                        xWarning = node.SelectSingleNode("//yahoo:Result/@warning", xMngr);
                        if (xWarning != null)
                        {
                            warning = xWarning.InnerText;
                            // Yahoo! embeds the depreciated <b> tag in this result (yuck!), we need to remove it.
                            warning = Regex.Replace(warning, @"<(.|\n)*?>", string.Empty);
                        }
                        else
                        {
                            warning = "";
                        }
                    }
                    catch
                    {
                        warning = "";
                    }

                    YahooAddress address = new YahooAddress(street, city, stateCode, zipCode, latitude, longitude, precison, warning, errorMessage);
                    ret.Add(address);
                }

            }
            catch (Exception e)
            {
                errorMessage = e.Message;
                YahooAddress address = new YahooAddress(street, city, stateCode, zipCode, latitude, longitude, precison, warning, errorMessage);
                ret.Add(address);
            }

            return ret;
        }
    }
}
