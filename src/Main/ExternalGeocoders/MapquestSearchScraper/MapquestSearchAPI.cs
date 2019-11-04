using System;
using System.Xml;
using USC.GISResearchLab.Common.Utils.Encoding;
using USC.GISResearchLab.Common.Utils.Strings;
using USC.GISResearchLab.Common.Utils.Web.URLs;

namespace USC.GISResearchLab.Geocoding.Core.ExternalGeocoders.MapquestSearchScraper
{

    public class MapquestSearchAPI
    {

        public static MapquestSearchScraperAddress Geocode(string name, string street, string city, string stateCode, string zipCode, string apiUrl)
        {

            MapquestSearchScraperAddress ret = null;
            if (String.IsNullOrEmpty(apiUrl))
            {
                apiUrl = "http://classic.mapquest.com/maps?";
            }

            string latitude = "";
            string longitude = "";
            string precison = "";
            string warning = "";
            string errorMessage = "";

            // Build URL request to be sent to Mapquest!
            string url = "";

            url += "cat=";
            if (!String.IsNullOrEmpty(name))
            {
                url += WebEncodingUtils.URLEncode(name);
            }

            // mapquest works best without the address
            //url += "&address=";
            //if (!String.IsNullOrEmpty(street))
            //{
            //    url += WebEncodingUtils.URLEncode(street);
            //}

            url += "&city=";
            if (!String.IsNullOrEmpty(city))
            {
                url += WebEncodingUtils.URLEncode(city);
            }

            url += "&state=";
            if (!String.IsNullOrEmpty(stateCode))
            {
                url += WebEncodingUtils.URLEncode(stateCode);
            }

            url += "&zipcode=";
            if (!String.IsNullOrEmpty(zipCode))
            {
                url += WebEncodingUtils.URLEncode(zipCode);
            }

            url = apiUrl + url;
            url = url.Replace(" ", "+");

            try
            {
                string content = URLUtils.GetUrlContent(url);

                if (!String.IsNullOrEmpty(content))
                {

                    int resultsStart = content.IndexOf("<div class=\"searchResults\"");

                    if (resultsStart > 0)
                    {
                        string results = content.Substring(resultsStart);
                        int resultsEnd = results.IndexOf("<form id=\"searchResultsForm\"");

                        if (resultsEnd > 0)
                        {

                            results = results.Substring(0, resultsEnd);

                            results = results.Replace("&", "&amp;");

                            results = results.Replace('\t', ' ');
                            results = results.Replace("\n", Environment.NewLine);

                            // there is an unclosed link in each result - close it
                            int linkEndStart = 0;
                            while (linkEndStart >= 0)
                            {
                                linkEndStart = results.IndexOf("class=\"fn org location-name link\">", linkEndStart);
                                if (linkEndStart >= 0)
                                {
                                    results = StringUtils.ReplaceAfterIndex(results, linkEndStart, "</span>", "</a></span>");
                                    linkEndStart++;
                                }
                            }


                            // there is an extra span in each result - remove it
                            int linkPhoneSpanStart = 0;
                            while (linkPhoneSpanStart >= 0)
                            {
                                linkPhoneSpanStart = results.IndexOf("class=\"phone-number\"", linkPhoneSpanStart);
                                if (linkPhoneSpanStart >= 0)
                                {
                                    results = StringUtils.ReplaceAfterIndex(results, linkPhoneSpanStart, "</span>", "");
                                    linkPhoneSpanStart++;
                                }
                            }


                            XmlDocument xmlDoc = new XmlDocument();
                            xmlDoc.LoadXml(results);

                            XmlNodeList resultList = xmlDoc.SelectNodes("//li");

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

                                        //recordId = firstNode.Attributes["id"].InnerText;

                                        XmlNode titleNode = firstNode.SelectSingleNode("//a[@class='fn org location-name link']");
                                        if (titleNode != null)
                                        {
                                            recordName = titleNode.InnerText;
                                            if (!String.IsNullOrEmpty(recordName))
                                            {
                                                recordName = URLUtils.HTMLDecode(recordName);
                                                recordName = URLUtils.HTMLDecode(recordName);
                                                recordName = recordName.Trim();
                                            }
                                        }

                                        XmlNode telephoneNode = firstNode.SelectSingleNode("//span[@class='phone-number']");
                                        if (telephoneNode != null)
                                        {
                                            recordPhone = telephoneNode.InnerText;
                                            if (!String.IsNullOrEmpty(recordPhone))
                                            {
                                                recordPhone = URLUtils.HTMLDecode(recordPhone);
                                                recordPhone = URLUtils.HTMLDecode(recordPhone);
                                                recordPhone = recordPhone.Trim();
                                            }
                                        }

                                        XmlNode addressNode = firstNode.SelectSingleNode("//span[@class='street-address']");
                                        if (addressNode != null)
                                        {
                                            recordAddress = addressNode.InnerText;
                                            if (!String.IsNullOrEmpty(recordAddress))
                                            {
                                                recordAddress = URLUtils.HTMLDecode(recordAddress);
                                                recordAddress = URLUtils.HTMLDecode(recordAddress);
                                                recordAddress = recordAddress.Trim();
                                            }
                                        }

                                        XmlNode cityNode = firstNode.SelectSingleNode("//span[@class='locality']");
                                        if (cityNode != null)
                                        {
                                            recordCity = cityNode.InnerText;
                                            if (!String.IsNullOrEmpty(recordCity))
                                            {
                                                recordCity = URLUtils.HTMLDecode(recordCity);
                                                recordCity = URLUtils.HTMLDecode(recordCity);
                                                recordCity = recordCity.Trim();
                                            }
                                        }

                                        XmlNode stateNode = firstNode.SelectSingleNode("//span[@class='region']");
                                        if (stateNode != null)
                                        {
                                            recordState = stateNode.InnerText;
                                            if (!String.IsNullOrEmpty(recordState))
                                            {
                                                recordState = URLUtils.HTMLDecode(recordState);
                                                recordState = URLUtils.HTMLDecode(recordState);
                                                recordCity.Trim();
                                            }
                                        }

                                        XmlNode zipNode = firstNode.SelectSingleNode("//span[@class='postal-code']");
                                        if (zipNode != null)
                                        {
                                            recordZip = zipNode.InnerText;
                                            if (!String.IsNullOrEmpty(recordZip))
                                            {
                                                recordZip = URLUtils.HTMLDecode(recordZip);
                                                recordZip = URLUtils.HTMLDecode(recordZip);
                                                recordZip.Trim();
                                            }
                                        }

                                        ret = new MapquestSearchScraperAddress(recordId, recordName, recordAddress, recordCity, recordState, recordZip, recordPhone, "0", "0", precison, warning, errorMessage);
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
                ret = new MapquestSearchScraperAddress(null, null, null, null, null, null, null, "0", "0", precison, warning, errorMessage);
            }

            return ret;
        }
    }
}
