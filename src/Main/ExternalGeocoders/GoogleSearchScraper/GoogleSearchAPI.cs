using System;
using System.Threading;
using USC.GISResearchLab.Common.Utils.Encoding;
using USC.GISResearchLab.Common.Utils.Web.URLs;

namespace USC.GISResearchLab.Geocoding.Core.ExternalGeocoders.GoogleSearchScraper
{

    public class GoogleSearchAPI
    {

        public static GoogleSearchScraperAddress Geocode(string name, string street, string city, string stateCode, string zipCode, string apiUrl)
        {

            GoogleSearchScraperAddress ret = null;
            if (String.IsNullOrEmpty(apiUrl))
            {
                apiUrl = "http://www.google.com/search?num=30&hl=en&newwindow=1&q=";
            }

            string latitude = "";
            string longitude = "";
            string precison = "";
            string warning = "";
            string errorMessage = "";

            // Build URL request to be sent to Yahoo!
            string url = "";
            if (!String.IsNullOrEmpty(name))
            {
                url += WebEncodingUtils.URLEncode(name);
            }
            if (!String.IsNullOrEmpty(street))
            {
                url += WebEncodingUtils.URLEncode(" " + street);
            }
            if (!String.IsNullOrEmpty(city))
            {
                url += WebEncodingUtils.URLEncode(", " + city);
            }
            if (!String.IsNullOrEmpty(stateCode))
            {
                url += WebEncodingUtils.URLEncode(" " + stateCode);
            }

            url = apiUrl + url;
            url += "&btnG=Search&aq=f&aqi=&aql=&oq=";
            url = url.Replace(" ", "+");

            try
            {
                Thread.Sleep(1000);
                string content = URLUtils.GetUrlContent(url);

                if (!String.IsNullOrEmpty(content))
                {
                    if (content.IndexOf("Place page") > 0)
                    {
                        int placePageStart = content.IndexOf("Place page");
                        int placePageBackup = 0;
                        if (placePageStart > 200)
                        {
                            placePageBackup = 200;
                        }
                        else
                        {
                            placePageBackup = placePageStart / 2;
                        }

                        string trimmed = content.Substring(content.IndexOf("Place page") - placePageBackup);

                        if (trimmed.IndexOf("<div class=gl") > 0)
                        {
                            trimmed = trimmed.Substring(0, trimmed.IndexOf("<div class=gl"));

                            string outputName = "";

                            if (trimmed.IndexOf("title=") > 0)
                            {
                                // get the name
                                int nameStart = trimmed.IndexOf("title=") + 7;
                                int nameEnd = trimmed.IndexOf("Place page");
                                int length = nameEnd - nameStart - 2;

                                if (length < 0)
                                {
                                    throw new Exception("NameLength < 0" + trimmed);
                                }

                                outputName = trimmed.Substring(nameStart, length);

                                outputName = URLUtils.HTMLDecode(outputName);
                                outputName = URLUtils.HTMLDecode(outputName);

                                trimmed = trimmed.Substring(trimmed.IndexOf("width:21em;margin-top:6px") + 27);

                                if (trimmed.IndexOf("<br>") > 0)
                                {
                                    string[] parts = trimmed.Split(new string[] { "<br>" }, StringSplitOptions.RemoveEmptyEntries);

                                    if (parts.Length == 3)
                                    {
                                        string outputAddress = parts[0];
                                        string outputCityStateZip = parts[1];
                                        string outputPhone = parts[2];

                                        if (!String.IsNullOrEmpty(outputPhone))
                                        {
                                            outputPhone = outputPhone.Trim();

                                            if (!String.IsNullOrEmpty(outputAddress))
                                            {
                                                outputAddress = outputAddress.Trim();

                                                if (!String.IsNullOrEmpty(outputCityStateZip))
                                                {

                                                    outputCityStateZip = outputCityStateZip.Trim();

                                                    int commaIndex = outputCityStateZip.IndexOf(',');

                                                    if (commaIndex > 0)
                                                    {
                                                        string outputCity = outputCityStateZip.Substring(0, commaIndex);

                                                        if (!String.IsNullOrEmpty(outputCity))
                                                        {

                                                            outputCity = outputCity.Trim();

                                                            string outputStateZip = outputCityStateZip.Substring(commaIndex + 1);

                                                            string outputState = "";
                                                            string outputZip = "";

                                                            string[] stateZip = outputStateZip.Split(new string[] { " ", }, StringSplitOptions.RemoveEmptyEntries);
                                                            if (stateZip.Length == 2)
                                                            {
                                                                outputState = stateZip[0];

                                                                if (!String.IsNullOrEmpty(outputState))
                                                                {

                                                                    outputState = outputState.Trim();

                                                                    outputZip = stateZip[1];

                                                                    if (!String.IsNullOrEmpty(outputZip))
                                                                    {

                                                                        outputZip = outputZip.Trim();
                                                                        ret = new GoogleSearchScraperAddress(outputName, outputAddress, outputCity, outputState, outputZip, outputPhone, "0", "0", precison, warning, errorMessage);
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        throw new Exception("Comma Index in City State < 0: " + outputCityStateZip);
                                                    }
                                                }
                                            }
                                        }
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
            }

            return ret;
        }
    }
}
