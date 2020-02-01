using Reimers.Map.Geocoding;
using System;
using System.Collections.Generic;
using System.Reflection;
using USC.GISResearchLab.AddressProcessing.Core.AddressNormalization.Implementations;
using USC.GISResearchLab.AddressProcessing.Core.Parsing.AddressParserManagers.Factories;
using USC.GISResearchLab.Common.Addresses;
using USC.GISResearchLab.Common.Core.Geocoders.GeocodingQueries;
using USC.GISResearchLab.Common.Geometries.Points;
using USC.GISResearchLab.Geocoding.Core.Configurations;
using USC.GISResearchLab.Geocoding.Core.ExternalGeocoders.Yahoo;
using USC.GISResearchLab.Geocoding.Core.Metadata.FeatureMatchingResults;
using USC.GISResearchLab.Geocoding.Core.Metadata.Qualities;
using USC.GISResearchLab.Geocoding.Core.OutputData;

namespace USC.GISResearchLab.Geocoding.Core.ExternalGeocoders.Bing
{
    public class ExternalBingGeocoder : AbstractSingleThreadedGeocoder, ICloneable
    {
        #region Properties

        public bool ShouldDoExhaustiveSearch { get; set; }

        #endregion

        public ExternalBingGeocoder() : base() { }

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
                List<BingAddress> resultList = BingAPI_V2.Geocode(query.StreetAddress.GetStreetAddressPortionAsString(), query.StreetAddress.City, query.StreetAddress.State, query.StreetAddress.ZIP);

                if (resultList != null)
                {
                    foreach (BingAddress result in resultList)
                    {
                        if (result != null)
                        {
                            Geocode geocode = new Geocode(Convert.ToDouble(query.BaseOptions.Version.ToString()));

                            geocode.InputAddress = query.StreetAddress;
                            geocode.ParsedAddress = query.StreetAddress;
                            geocode.Geometry = new Point(Convert.ToDouble(result.Longitude), Convert.ToDouble(result.Latitude));

                            //StreetAddress returnedAddress = new AddressNormalizer().NormalizeStreetAddress(result.Street, result.City, result.StateCode, result.ZipCode);

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

                            if (!String.IsNullOrEmpty(result.Warning))
                            {
                                Serilog.Log.Warning(MethodBase.GetCurrentMethod().GetType().Name + " " + MethodBase.GetCurrentMethod().Name + ": " + result.Warning);
                            }

                            switch (result.Precision)
                            {
                                case "Interpolation":
                                    if (result.MatchType.ToLower().Contains("uphierarchy"))
                                    {
                                        if (!String.IsNullOrEmpty(geocode.MatchedAddress.StreetName))
                                        {
                                            geocode.FM_GeographyType = FeatureMatchingGeographyType.StreetCentroid;
                                            geocode.GeocodeQualityType = GeocodeQualityType.StreetCentroid;
                                        }
                                        else if (geocode.MatchedAddress.HasZip)
                                        {
                                            geocode.FM_GeographyType = FeatureMatchingGeographyType.USPSZip;
                                            geocode.GeocodeQualityType = GeocodeQualityType.ZCTACentroid;
                                        }
                                        else if (geocode.MatchedAddress.HasCity)
                                        {
                                            geocode.FM_GeographyType = FeatureMatchingGeographyType.City;
                                            geocode.GeocodeQualityType = GeocodeQualityType.CityCentroid;
                                        }
                                        else if (geocode.MatchedAddress.HasState)
                                        {
                                            geocode.FM_GeographyType = FeatureMatchingGeographyType.State;
                                            geocode.GeocodeQualityType = GeocodeQualityType.StateCentroid;
                                        }
                                        else
                                        {
                                            geocode.FM_GeographyType = FeatureMatchingGeographyType.Unmatchable;
                                            geocode.GeocodeQualityType = GeocodeQualityType.Unmatchable;
                                        }
                                    }
                                    else
                                    {
                                        geocode.FM_GeographyType = FeatureMatchingGeographyType.StreetSegment;
                                        geocode.GeocodeQualityType = GeocodeQualityType.AddressRangeInterpolation;
                                    }
                                    break;
                                case "InterpolationOffset":
                                    if (result.MatchType.ToLower().Contains("uphierarchy"))
                                    {
                                        if (!String.IsNullOrEmpty(geocode.MatchedAddress.StreetName))
                                        {
                                            geocode.FM_GeographyType = FeatureMatchingGeographyType.StreetCentroid;
                                            geocode.GeocodeQualityType = GeocodeQualityType.StreetCentroid;
                                        }
                                        else if (geocode.MatchedAddress.HasZip)
                                        {
                                            geocode.FM_GeographyType = FeatureMatchingGeographyType.USPSZip;
                                            geocode.GeocodeQualityType = GeocodeQualityType.ZCTACentroid;
                                        }
                                        else if (geocode.MatchedAddress.HasCity)
                                        {
                                            geocode.FM_GeographyType = FeatureMatchingGeographyType.City;
                                            geocode.GeocodeQualityType = GeocodeQualityType.CityCentroid;
                                        }
                                        else if (geocode.MatchedAddress.HasState)
                                        {
                                            geocode.FM_GeographyType = FeatureMatchingGeographyType.State;
                                            geocode.GeocodeQualityType = GeocodeQualityType.StateCentroid;
                                        }
                                        else
                                        {
                                            geocode.FM_GeographyType = FeatureMatchingGeographyType.Unmatchable;
                                            geocode.GeocodeQualityType = GeocodeQualityType.Unmatchable;
                                        }
                                    }
                                    else
                                    {
                                        geocode.FM_GeographyType = FeatureMatchingGeographyType.StreetSegment;
                                        geocode.GeocodeQualityType = GeocodeQualityType.AddressRangeInterpolation;
                                    }
                                    break;
                                case "Parcel":
                                    geocode.FM_GeographyType = FeatureMatchingGeographyType.Parcel;
                                    geocode.GeocodeQualityType = GeocodeQualityType.ExactParcelCentroid;
                                    break;
                                case "Rooftop":
                                    geocode.FM_GeographyType = FeatureMatchingGeographyType.BuildingCentroid;
                                    geocode.GeocodeQualityType = GeocodeQualityType.BuildingCentroid;
                                    break;
                            }


                            DateTime endTime = DateTime.Now;

                            TimeSpan timeTaken = endTime.Subtract(startTime);

                            geocode.TotalTimeTaken = timeTaken;

                            ret.AddGeocode(geocode);
                        }
                    }
                }


                // if there are two best-geocodes then this is an ambigous result

                if (ret.GeocodeCollection.Geocodes.Count > 0)
                {
                    int bestCount = 0;
                    IGeocode bestGeocode = ret.BestGeocodeHierarchyFeatureType;
                    foreach (IGeocode geocode in ret.GeocodeCollection.Geocodes)
                    {
                        if (bestGeocode.FM_GeographyType == geocode.FM_GeographyType)
                        {
                            bestCount++;
                        }
                    }

                    if (bestCount > 1)
                    {
                        bestGeocode.FM_ResultType = FeatureMatchingResultType.Ambiguous;
                    }
                    else
                    {
                        // if it is the same number and street name, it's exact
                        if (String.Compare(bestGeocode.InputAddress.Number, bestGeocode.MatchedAddress.Number, true) == 0)
                        {
                            bestGeocode.FM_ResultType = FeatureMatchingResultType.Success;
                        }
                        else
                        {
                            // if it's not the same number and name, it's either uphierarchy or a nearby match
                            if (bestGeocode.FM_GeographyType == FeatureMatchingGeographyType.BuildingCentroid ||
                                bestGeocode.FM_GeographyType == FeatureMatchingGeographyType.Parcel ||
                                bestGeocode.FM_GeographyType == FeatureMatchingGeographyType.StreetSegment)
                            {
                                bestGeocode.FM_ResultType = FeatureMatchingResultType.Nearby;
                            }
                            else
                            {
                                bestGeocode.FM_ResultType = FeatureMatchingResultType.Success;
                            }
                        }
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

        public new SingleThreadedFeatureHierarchyGeocoder Clone()
        {
            return (SingleThreadedFeatureHierarchyGeocoder)MemberwiseClone();
        }

        #endregion
    }
}
