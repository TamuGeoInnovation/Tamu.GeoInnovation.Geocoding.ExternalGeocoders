using System;
using System.Xml;
using USC.GISResearchLab.Common.Utils.Encoding;
using USC.GISResearchLab.Common.Utils.Web.URLs;

namespace USC.GISResearchLab.Geocoding.Core.ExternalGeocoders.YahooSearchScraper
{

    public class YahooSearchAPI
    {

        public static YahooSearchScraperAddress Geocode(string name, string street, string city, string stateCode, string zipCode, string apiUrl)
        {

            YahooSearchScraperAddress ret = null;
            if (String.IsNullOrEmpty(apiUrl))
            {
                apiUrl = "http://local.yahoo.com/results?";
            }

            string latitude = "";
            string longitude = "";
            string precison = "";
            string warning = "";
            string errorMessage = "";

            // Build URL request to be sent to Yahoo!
            string url = "";
            url += "stx=";
            if (!String.IsNullOrEmpty(name))
            {
                url += WebEncodingUtils.URLEncode(name);
            }

            url += "&csz=";
            if (!String.IsNullOrEmpty(street))
            {
                url += WebEncodingUtils.URLEncode(street);
            }

            if (!String.IsNullOrEmpty(city))
            {
                url += WebEncodingUtils.URLEncode(", " + city);
            }

            if (!String.IsNullOrEmpty(stateCode))
            {
                url += WebEncodingUtils.URLEncode(" " + stateCode);
            }

            if (!String.IsNullOrEmpty(zipCode))
            {
                url += WebEncodingUtils.URLEncode(" " + zipCode);
            }

            url = apiUrl + url;
            url = url.Replace(" ", "+");

            try
            {
                string content = URLUtils.GetUrlContent(url);

                if (!String.IsNullOrEmpty(content))
                {

                    int resultsStart = content.IndexOf("<table id=\"yls-rs-res\"");

                    if (resultsStart > 0)
                    {
                        string results = content.Substring(resultsStart);
                        int resultsEnd = results.IndexOf("</table>");

                        if (resultsEnd > 0)
                        {

                            results = results.Substring(0, resultsEnd + 8);


                            //int firstResultsStart = content.IndexOf("<tbody");
                            //string firstResult = content.Substring(firstResultsStart);
                            //int firstResultEnd = firstResult.IndexOf("</tbody>");

                            //firstResult = firstResult.Substring(0, firstResultEnd + 8);

                            //firstResult = firstResult.Replace("&", "&amp;");

                            results = results.Replace("&", "&amp;");
                            XmlDocument xmlDoc = new XmlDocument();
                            xmlDoc.LoadXml(results);

                            XmlNodeList resultList = xmlDoc.SelectNodes("//tbody");

                            if (resultList != null)
                            {
                                if (resultList.Count > 0)
                                {
                                    XmlNode firstNode = resultList[0];
                                    if (firstNode != null)
                                    {
                                        string recordId = "";
                                        string recordName = "";
                                        string recordAddress = "";
                                        string recordCity = "";
                                        string recordState = "";
                                        string recordZip = "";
                                        string recordPhone = "";

                                        recordId = firstNode.Attributes["id"].InnerText;

                                        XmlNode titleNode = firstNode.SelectSingleNode("//span[@class='yls-rs-listing-title']");
                                        if (titleNode != null)
                                        {
                                            recordName = titleNode.Attributes["content"].InnerText;
                                            recordName = URLUtils.HTMLDecode(recordName);
                                            recordName = URLUtils.HTMLDecode(recordName);
                                        }

                                        XmlNode telephoneNode = firstNode.SelectSingleNode("//span[@property='vcard:tel']");
                                        if (telephoneNode != null)
                                        {
                                            recordPhone = telephoneNode.InnerText;
                                            recordPhone = URLUtils.HTMLDecode(recordPhone);
                                            recordPhone = URLUtils.HTMLDecode(recordPhone);
                                        }

                                        XmlNode addressNode = firstNode.SelectSingleNode("//span[@property='vcard:street-address']");
                                        if (addressNode != null)
                                        {
                                            recordAddress = addressNode.Attributes["content"].InnerText;
                                            recordAddress = URLUtils.HTMLDecode(recordAddress);
                                            recordAddress = URLUtils.HTMLDecode(recordAddress);
                                        }

                                        XmlNode cityNode = firstNode.SelectSingleNode("//span[@property='vcard:locality']");
                                        if (cityNode != null)
                                        {
                                            recordCity = cityNode.Attributes["content"].InnerText;
                                            recordCity = URLUtils.HTMLDecode(recordCity);
                                            recordCity = URLUtils.HTMLDecode(recordCity);
                                        }

                                        XmlNode stateNode = firstNode.SelectSingleNode("//span[@property='vcard:region']");
                                        if (stateNode != null)
                                        {
                                            recordState = stateNode.Attributes["content"].InnerText;
                                            recordState = URLUtils.HTMLDecode(recordState);
                                            recordState = URLUtils.HTMLDecode(recordState);
                                        }

                                        ret = new YahooSearchScraperAddress(recordId, recordName, recordAddress, recordCity, recordState, recordZip, recordPhone, "0", "0", precison, warning, errorMessage);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {

                errorMessage = e.Message;
                ret = new YahooSearchScraperAddress(null, null, null, null, null, null, null, "0", "0", precison, warning, errorMessage);
            }

            return ret;
        }
    }
}
