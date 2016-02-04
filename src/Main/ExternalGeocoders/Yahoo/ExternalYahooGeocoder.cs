using System;
using System.Collections.Generic;
using Reimers.Map.Geocoding;
using USC.GISResearchLab.AddressProcessing.Core.AddressNormalization.Implementations;
using USC.GISResearchLab.AddressProcessing.Core.Parsing.AddressParserManagers.Factories;
using USC.GISResearchLab.Common.Addresses;
using USC.GISResearchLab.Common.Core.Geocoders.GeocodingQueries;
using USC.GISResearchLab.Common.Geometries.Points;
//using USC.GISResearchLab.Geocoding.Core.Algorithms.FeatureMatchingMethods;
using USC.GISResearchLab.Geocoding.Core.Metadata.FeatureMatchingResults;
using USC.GISResearchLab.Geocoding.Core.Metadata.Qualities;
using USC.GISResearchLab.Geocoding.Core.OutputData;
using USC.GISResearchLab.Geocoding.Core.Configurations;
using USC.GISResearchLab.Geocoding.Core.Algorithms.FeatureMatchingMethods;

namespace USC.GISResearchLab.Geocoding.Core.ExternalGeocoders.Yahoo
{
    public class ExternalYahooGeocoder : AbstractSingleThreadedGeocoder, ICloneable
    {
        #region Properties

        public bool ShouldDoExhaustiveSearch { get; set; }

        #endregion

        public ExternalYahooGeocoder() : base() { }

        public override GeocodeResultSet Geocode(GeocodingQuery query, GeocoderConfiguration geocoderConfiguration)
        {

            GeocodeResultSet ret = new GeocodeResultSet();
            ret.StartTime = DateTime.Now;
            try
            {
                Address address = new Address();
                address.AdministrativeArea = query.StreetAddress.State;
                address.City = query.StreetAddress.City;
                address.Country = "US";
                address.Street = query.StreetAddress.GetStreetAddressPortionAsString();

                DateTime startTime = DateTime.Now;
                List<YahooAddress> resultList = YahooAPI_V2.Geocode(query.StreetAddress.GetStreetAddressPortionAsString(), query.StreetAddress.City, query.StreetAddress.State, query.StreetAddress.ZIP, "");

                if (resultList != null)
                {

                    bool isAmbigous = false;
                    if (resultList.Count > 1)
                    {
                        isAmbigous = true;
                    }

                    foreach (YahooAddress result in resultList)
                    {
                        Geocode geocode = new Geocode(Convert.ToDouble(query.BaseOptions.Version.ToString()));

                        if (!String.IsNullOrEmpty(result.ErrorMessage))
                        {
                            geocode.ErrorMessage = result.ErrorMessage;
                            geocode.ExceptionOccurred = true;
                        }

                        geocode.InputAddress = query.StreetAddress;
                        geocode.ParsedAddress = query.StreetAddress;

                       if (isAmbigous)
                        {
                            geocode.FM_ResultType = FeatureMatchingResultType.Ambiguous;
                        }
                        else
                        {
                            geocode.FM_ResultType = FeatureMatchingResultType.Success;
                        }



                        StreetAddress returnedAddress = null;
                        if (Convert.ToDouble(geocoderConfiguration.Version.ToString()) >= 2.95)
                        {

                            AddressComponents addressComponentsStreetAddress = AddressComponents.Number
                                | AddressComponents.NumberFractional
                                | AddressComponents.PostArticle
                                | AddressComponents.PostDirectional
                                | AddressComponents.PostQualifier
                                | AddressComponents.PreArticle
                                | AddressComponents.PreDirectional
                                | AddressComponents.PreQualifier
                                | AddressComponents.PreType
                                | AddressComponents.StreetName
                                | AddressComponents.Suffix
                                | AddressComponents.SuiteNumber
                                | AddressComponents.SuiteType;

                            AddressComponents addressComponentsCity = AddressComponents.City;
                            AddressComponents addressComponentsState = AddressComponents.State;
                            AddressComponents addressComponentsZip = AddressComponents.Zip | AddressComponents.ZipPlus4;


                            AddressNormalizer addressNormalizerStreetAddress = new AddressNormalizer(AddressParserType.TokenBased, addressComponentsStreetAddress, Convert.ToDouble(geocoderConfiguration.Version.ToString()), AddressFormatType.LACounty);
                            AddressNormalizer addressNormalizerCity = new AddressNormalizer(AddressParserType.TokenBased, addressComponentsCity, Convert.ToDouble(geocoderConfiguration.Version.ToString()), AddressFormatType.LACounty);
                            AddressNormalizer addressNormalizerState = new AddressNormalizer(AddressParserType.TokenBased, addressComponentsState, Convert.ToDouble(geocoderConfiguration.Version.ToString()), AddressFormatType.LACounty);
                            AddressNormalizer addressNormalizerZip = new AddressNormalizer(AddressParserType.TokenBased, addressComponentsZip, Convert.ToDouble(geocoderConfiguration.Version.ToString()), AddressFormatType.LACounty);

                            returnedAddress = addressNormalizerStreetAddress.Normalize(result.Street);
                            addressNormalizerCity.Normalize(returnedAddress, result.City, null);
                            addressNormalizerState.Normalize(returnedAddress, result.StateCode, null);
                            addressNormalizerZip.Normalize(returnedAddress, result.ZipCode, null);
                        }
                        else
                        {
                            AddressNormalizer addressNormalizer = new AddressNormalizer(AddressParserType.TokenBased, Convert.ToDouble(geocoderConfiguration.Version.ToString()), AddressFormatType.LACounty);
                            returnedAddress = addressNormalizer.NormalizeStreetAddress(result.Street, result.City, result.StateCode, result.ZipCode);

                        }

                        geocode.MatchedAddress = RelaxableStreetAddress.FromStreetAddress(returnedAddress);
                        //geocode.MatchedAddress.City = result.City;
                        //geocode.MatchedAddress.State = result.StateCode;
                        //geocode.MatchedAddress.ZIP = result.ZipCode;

                        geocode.MatchedFeatureAddress = RelaxableStreetAddress.FromStreetAddress(returnedAddress);
                        //geocode.MatchedFeatureAddress.City = result.City;
                        //geocode.MatchedFeatureAddress.State = result.StateCode;
                        //geocode.MatchedFeatureAddress.ZIP = result.ZipCode;


                        geocode.Geometry = new Point(Convert.ToDouble(result.Longitude), Convert.ToDouble(result.Latitude));



                        switch (result.Precision)
                        {
                            case "90":
                                geocode.FM_GeographyType = FeatureMatchingGeographyType.Parcel;
                                geocode.MatchedFeature.FeatureMatchTypes = FeatureMatchTypes.Exact;
                                geocode.GeocodeQualityType = GeocodeQualityType.ExactParcelCentroid;
                                break;
                            case "87":
                                geocode.FM_GeographyType = FeatureMatchingGeographyType.StreetSegment;
                                geocode.MatchedFeature.FeatureMatchTypes = FeatureMatchTypes.Exact;
                                geocode.GeocodeQualityType = GeocodeQualityType.AddressRangeInterpolation;
                                break;
                            case "86":
                                geocode.FM_GeographyType = FeatureMatchingGeographyType.StreetSegment;
                                geocode.MatchedFeature.FeatureMatchTypes = FeatureMatchTypes.Nearby;
                                geocode.GeocodeQualityType = GeocodeQualityType.AddressRangeInterpolation;
                                break;
                            case "85":
                                geocode.FM_GeographyType = FeatureMatchingGeographyType.StreetSegment;
                                geocode.MatchedFeature.FeatureMatchTypes = FeatureMatchTypes.Nearby;
                                geocode.GeocodeQualityType = GeocodeQualityType.AddressRangeInterpolation;
                                break;
                            case "84":
                                geocode.FM_GeographyType = FeatureMatchingGeographyType.StreetSegment;
                                geocode.MatchedFeature.FeatureMatchTypes = FeatureMatchTypes.Nearby;
                                geocode.GeocodeQualityType = GeocodeQualityType.AddressRangeInterpolation;
                                break;
                            case "82":
                                geocode.FM_GeographyType = FeatureMatchingGeographyType.StreetIntersection;
                                geocode.MatchedFeature.FeatureMatchTypes = FeatureMatchTypes.Exact;
                                geocode.GeocodeQualityType = GeocodeQualityType.StreetIntersection;
                                break;
                            case "80":
                                geocode.FM_GeographyType = FeatureMatchingGeographyType.StreetIntersection;
                                geocode.MatchedFeature.FeatureMatchTypes = FeatureMatchTypes.Nearby;
                                geocode.GeocodeQualityType = GeocodeQualityType.StreetIntersection;
                                break;
                            case "75":
                                geocode.FM_GeographyType = FeatureMatchingGeographyType.USPSZipPlus4;
                                geocode.MatchedFeature.FeatureMatchTypes = FeatureMatchTypes.Exact;
                                geocode.GeocodeQualityType = GeocodeQualityType.ZCTAPlus4Centroid;
                                break;
                            case "74":
                                geocode.FM_GeographyType = FeatureMatchingGeographyType.USPSZipPlus4;
                                geocode.MatchedFeature.FeatureMatchTypes = FeatureMatchTypes.Exact;
                                geocode.GeocodeQualityType = GeocodeQualityType.ZCTAPlus4Centroid;
                                break;
                            case "72":
                                geocode.FM_GeographyType = FeatureMatchingGeographyType.StreetCentroid;
                                geocode.MatchedFeature.FeatureMatchTypes = FeatureMatchTypes.Exact;
                                geocode.GeocodeQualityType = GeocodeQualityType.StreetCentroid;
                                break;
                            case "71":
                                geocode.FM_GeographyType = FeatureMatchingGeographyType.StreetCentroid;
                                geocode.MatchedFeature.FeatureMatchTypes = FeatureMatchTypes.Exact;
                                geocode.GeocodeQualityType = GeocodeQualityType.StreetCentroid;
                                break;
                            case "70":
                                geocode.FM_GeographyType = FeatureMatchingGeographyType.StreetCentroid;
                                geocode.MatchedFeature.FeatureMatchTypes = FeatureMatchTypes.Nearby;
                                geocode.GeocodeQualityType = GeocodeQualityType.StreetCentroid;
                                break;
                            case "64":
                                geocode.FM_GeographyType = FeatureMatchingGeographyType.ZCTAPlus2;
                                geocode.MatchedFeature.FeatureMatchTypes = FeatureMatchTypes.Exact;
                                geocode.GeocodeQualityType = GeocodeQualityType.ZCTAPlus2Centroid;
                                break;
                            case "63":
                                geocode.FM_GeographyType = FeatureMatchingGeographyType.Parcel;
                                geocode.MatchedFeature.FeatureMatchTypes = FeatureMatchTypes.Exact;
                                geocode.GeocodeQualityType = GeocodeQualityType.ExactParcelCentroid;
                                break;
                            case "62":
                                geocode.FM_GeographyType = FeatureMatchingGeographyType.Parcel;
                                geocode.MatchedFeature.FeatureMatchTypes = FeatureMatchTypes.Exact;
                                geocode.GeocodeQualityType = GeocodeQualityType.ExactParcelCentroid;
                                break;
                            case "60":
                                geocode.FM_GeographyType = FeatureMatchingGeographyType.USPSZip;
                                geocode.MatchedFeature.FeatureMatchTypes = FeatureMatchTypes.Exact;
                                geocode.GeocodeQualityType = GeocodeQualityType.ZCTACentroid;
                                break;
                            case "59":
                                geocode.FM_GeographyType = FeatureMatchingGeographyType.USPSZip;
                                geocode.MatchedFeature.FeatureMatchTypes = FeatureMatchTypes.Exact;
                                geocode.GeocodeQualityType = GeocodeQualityType.ZCTACentroid;
                                break;
                            case "50":
                                geocode.FM_GeographyType = FeatureMatchingGeographyType.City;
                                geocode.MatchedFeature.FeatureMatchTypes = FeatureMatchTypes.Exact;
                                geocode.GeocodeQualityType = GeocodeQualityType.CityCentroid;
                                break;
                            case "49":
                                geocode.FM_GeographyType = FeatureMatchingGeographyType.City;
                                geocode.MatchedFeature.FeatureMatchTypes = FeatureMatchTypes.Exact;
                                geocode.GeocodeQualityType = GeocodeQualityType.CityCentroid;
                                break;
                            case "40":
                                geocode.FM_GeographyType = FeatureMatchingGeographyType.City;
                                geocode.MatchedFeature.FeatureMatchTypes = FeatureMatchTypes.Exact;
                                geocode.GeocodeQualityType = GeocodeQualityType.CityCentroid;
                                break;
                            case "39":
                                geocode.FM_GeographyType = FeatureMatchingGeographyType.City;
                                geocode.MatchedFeature.FeatureMatchTypes = FeatureMatchTypes.Exact;
                                geocode.GeocodeQualityType = GeocodeQualityType.CityCentroid;
                                break;
                            case "30":
                                geocode.FM_GeographyType = FeatureMatchingGeographyType.County;
                                geocode.MatchedFeature.FeatureMatchTypes = FeatureMatchTypes.Exact;
                                geocode.GeocodeQualityType = GeocodeQualityType.CountyCentroid;
                                break;
                            case "29":
                                geocode.FM_GeographyType = FeatureMatchingGeographyType.County;
                                geocode.MatchedFeature.FeatureMatchTypes = FeatureMatchTypes.Exact;
                                geocode.GeocodeQualityType = GeocodeQualityType.CountyCentroid;
                                break;
                            case "20":
                                geocode.FM_GeographyType = FeatureMatchingGeographyType.State;
                                geocode.MatchedFeature.FeatureMatchTypes = FeatureMatchTypes.Exact;
                                geocode.GeocodeQualityType = GeocodeQualityType.StateCentroid;
                                break;
                            case "19":
                                geocode.FM_GeographyType = FeatureMatchingGeographyType.State;
                                geocode.MatchedFeature.FeatureMatchTypes = FeatureMatchTypes.Exact;
                                geocode.GeocodeQualityType = GeocodeQualityType.StateCentroid;
                                break;
                            case "10":
                                geocode.FM_GeographyType = FeatureMatchingGeographyType.State;
                                geocode.MatchedFeature.FeatureMatchTypes = FeatureMatchTypes.Exact;
                                geocode.GeocodeQualityType = GeocodeQualityType.StateCentroid;
                                break;
                            case "9":
                                geocode.FM_GeographyType = FeatureMatchingGeographyType.State;
                                geocode.MatchedFeature.FeatureMatchTypes = FeatureMatchTypes.Exact;
                                geocode.GeocodeQualityType = GeocodeQualityType.StateCentroid;
                                break;
                            case "0":
                                geocode.FM_GeographyType = FeatureMatchingGeographyType.Unknown;
                                geocode.MatchedFeature.FeatureMatchTypes = FeatureMatchTypes.Unknown;
                                geocode.GeocodeQualityType = GeocodeQualityType.Unknown;
                                break;
                            default:
                                geocode.FM_GeographyType = FeatureMatchingGeographyType.Unknown;
                                geocode.MatchedFeature.FeatureMatchTypes = FeatureMatchTypes.Unknown;
                                geocode.GeocodeQualityType = GeocodeQualityType.Unknown;
                                break;
                            
                        }


                        // the code below was for the local.yahoo.com API which has been deprecated
                        //if (!String.IsNullOrEmpty(result.Warning))
                        //{
                        //    geocode.ErrorMessage = result.Warning;

                        //    if (result.Warning.ToLower().IndexOf("nearby") >= 0)
                        //    {
                        //        geocode.FeatureMatchingResultType = FeatureMatchingResultType.Nearby;
                        //    }
                        //    else if (result.Warning.ToLower().IndexOf("closest") >= 0)
                        //    {
                        //        geocode.FeatureMatchingResultType = FeatureMatchingResultType.Nearby;
                        //    }
                        //    else
                        //    {
                        //        geocode.FeatureMatchingResultType = FeatureMatchingResultType.Nearby;
                        //    }
                        //}
                        //else if (isAmbigous)
                        //{
                        //    geocode.FeatureMatchingResultType = FeatureMatchingResultType.Ambiguous;
                        //}
                        //else
                        //{
                        //    geocode.FeatureMatchingResultType = FeatureMatchingResultType.Success;
                        //}


                        ////StreetAddress returnedAddress = new AddressNormalizer().NormalizeStreetAddress(result.Street);

                        //StreetAddress returnedAddress = null;
                        //if (geocoderConfiguration.Version >= 2.95)
                        //{

                        //    AddressComponents addressComponentsStreetAddress = AddressComponents.Number
                        //        | AddressComponents.NumberFractional
                        //        | AddressComponents.PostArticle
                        //        | AddressComponents.PostDirectional
                        //        | AddressComponents.PostQualifier
                        //        | AddressComponents.PreArticle
                        //        | AddressComponents.PreDirectional
                        //        | AddressComponents.PreQualifier
                        //        | AddressComponents.PreType
                        //        | AddressComponents.StreetName
                        //        | AddressComponents.Suffix
                        //        | AddressComponents.SuiteNumber
                        //        | AddressComponents.SuiteType;

                        //    AddressComponents addressComponentsCity = AddressComponents.City;
                        //    AddressComponents addressComponentsState = AddressComponents.State;
                        //    AddressComponents addressComponentsZip = AddressComponents.Zip | AddressComponents.ZipPlus4;


                        //    AddressNormalizer addressNormalizerStreetAddress = new AddressNormalizer(AddressParserType.TokenBased, addressComponentsStreetAddress, geocoderConfiguration.Version, AddressFormatType.LACounty);
                        //    AddressNormalizer addressNormalizerCity = new AddressNormalizer(AddressParserType.TokenBased, addressComponentsCity, geocoderConfiguration.Version, AddressFormatType.LACounty);
                        //    AddressNormalizer addressNormalizerState = new AddressNormalizer(AddressParserType.TokenBased, addressComponentsState, geocoderConfiguration.Version, AddressFormatType.LACounty);
                        //    AddressNormalizer addressNormalizerZip = new AddressNormalizer(AddressParserType.TokenBased, addressComponentsZip, geocoderConfiguration.Version, AddressFormatType.LACounty);

                        //    returnedAddress = addressNormalizerStreetAddress.Normalize(result.Street);
                        //    addressNormalizerCity.Normalize(returnedAddress, result.City, null);
                        //    addressNormalizerState.Normalize(returnedAddress, result.StateCode, null);
                        //    addressNormalizerZip.Normalize(returnedAddress, result.ZipCode, null);
                        //}
                        //else
                        //{
                        //    AddressNormalizer addressNormalizer = new AddressNormalizer(AddressParserType.TokenBased, geocoderConfiguration.Version, AddressFormatType.LACounty);
                        //    returnedAddress = addressNormalizer.NormalizeStreetAddress(result.Street, result.City, result.StateCode, result.ZipCode);

                        //}

                        //geocode.MatchedAddress = RelaxableStreetAddress.FromStreetAddress(returnedAddress);
                        ////geocode.MatchedAddress.City = result.City;
                        ////geocode.MatchedAddress.State = result.StateCode;
                        ////geocode.MatchedAddress.ZIP = result.ZipCode;

                        //geocode.MatchedFeatureAddress = RelaxableStreetAddress.FromStreetAddress(returnedAddress);
                        ////geocode.MatchedFeatureAddress.City = result.City;
                        ////geocode.MatchedFeatureAddress.State = result.StateCode;
                        ////geocode.MatchedFeatureAddress.ZIP = result.ZipCode;


                        //geocode.Geometry = new Point(Convert.ToDouble(result.Longitude), Convert.ToDouble(result.Latitude));

                        

                        //switch (result.Precision)
                        //{
                        //    case "address":
                        //        geocode.FeatureMatchingGeographyType = FeatureMatchingGeographyType.StreetSegment;
                        //        break;
                        //    case "state":
                        //        geocode.FeatureMatchingGeographyType = FeatureMatchingGeographyType.State;
                        //        break;
                        //    case "country":
                        //        geocode.FeatureMatchingGeographyType = FeatureMatchingGeographyType.Country;
                        //        break;
                        //    case "zip+4":
                        //        geocode.FeatureMatchingGeographyType = FeatureMatchingGeographyType.USPSZipPlus4;
                        //        break;
                        //    case "zip+2":
                        //        geocode.FeatureMatchingGeographyType = FeatureMatchingGeographyType.USPSZipPlus2;
                        //        break;
                        //    case "zip":
                        //        geocode.FeatureMatchingGeographyType = FeatureMatchingGeographyType.USPSZip;
                        //        break;
                        //    case "street":
                        //        geocode.FeatureMatchingGeographyType = FeatureMatchingGeographyType.StreetCentroid;
                        //        break;
                        //    case "city":
                        //        geocode.FeatureMatchingGeographyType = FeatureMatchingGeographyType.City;
                        //        break;
                        //}

                        DateTime endTime = DateTime.Now;

                        TimeSpan timeTaken = endTime.Subtract(startTime);

                        geocode.TotalTimeTaken = timeTaken;

                        ret.AddGeocode(geocode);
                    }
                }

                ret.EndTime = DateTime.Now;
                ret.Statistics.EndTime = DateTime.Now;
                ret.TimeTaken = TimeSpan.FromTicks(ret.EndTime.Ticks - ret.StartTime.Ticks);

            }
            catch (Exception e)
            {
                ret.Resultstring = e.GetType() + ": " + e.Message;
                ret.GeocodeQualityType = GeocodeQualityType.Unmatchable;
            }
            return ret;
        }

        #region ICloneable Members

        object ICloneable.Clone()
        {
            return Clone();
        }

        public virtual SingleThreadedFeatureHierarchyGeocoder Clone()
        {
            return (SingleThreadedFeatureHierarchyGeocoder)MemberwiseClone();
        }

        #endregion
    }
}
