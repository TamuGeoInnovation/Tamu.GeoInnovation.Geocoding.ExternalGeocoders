using System;
using Reimers.Map.Geocoding;
using USC.GISResearchLab.AddressProcessing.Core.AddressNormalization.Implementations;
using USC.GISResearchLab.AddressProcessing.Core.Parsing.AddressParserManagers.Factories;
using USC.GISResearchLab.Common.Addresses;
using USC.GISResearchLab.Common.Core.Geocoders.GeocodingQueries;
using USC.GISResearchLab.Common.Geometries.Points;
using USC.GISResearchLab.Geocoding.Core.Metadata.FeatureMatchingResults;
using USC.GISResearchLab.Geocoding.Core.Metadata.Qualities;
using USC.GISResearchLab.Geocoding.Core.OutputData;
using USC.GISResearchLab.Geocoding.Core.Configurations;

//using USC.GISResearchLab.Geocoding.Core.BingTokenService;



namespace USC.GISResearchLab.Geocoding.Core.ExternalGeocoders.Google
{
    public class ExternalGoogleGeocoder : AbstractSingleThreadedGeocoder, ICloneable
    {
        #region Properties

        public bool ShouldDoExhaustiveSearch { get; set; }

        #endregion

        public ExternalGoogleGeocoder() : base() { }

        public override GeocodeResultSet Geocode(GeocodingQuery query, GeocoderConfiguration geocoderConfiguration)
        {

            GeocodeResultSet ret = new GeocodeResultSet();
            ret.StartTime = DateTime.Now;
            try
            {
                Address address = new Address();
                address.Region = query.StreetAddress.State;
                address.City = query.StreetAddress.City;
                address.Country = "US";
                address.Street = query.StreetAddress.GetStreetAddressPortionAsString();

                address.Zip = query.StreetAddress.ZIP;
                if (!String.IsNullOrEmpty(query.StreetAddress.ZIPPlus4))
                {
                    address.Zip += "-" + query.StreetAddress.ZIPPlus4;
                }

                address.Zip = query.StreetAddress.ZIP;
                if (!String.IsNullOrEmpty(query.StreetAddress.ZIPPlus4))
                {
                    address.Zip += "-" + query.StreetAddress.ZIPPlus4;
                }

                //address.City = "Cairo";
                //address.Country = "EG";
                //address.Street = query.StreetAddress.StreetName;

               

                DateTime startTime = DateTime.Now;

                // limit the query frequency to 1 every second
                //Thread.Sleep(1000);

                GoogleResult googleResult = null;

                if (String.Compare(address.Country, "EG", true) == 0)
                {
                    System.Globalization.CultureInfo language = new System.Globalization.CultureInfo("ar-EG");
                    googleResult = GoogleGeocoder.Geocode(address, language, "");
                }
                else
                {
                    googleResult = GoogleGeocoder.Geocode(address, "");
                }

                if (googleResult.Status == GeocodeStatus.G_GEO_SUCCESS)
                {
                    if (googleResult != null)
                    {
                        bool isAmbigous = false;
                        if (googleResult.Locations.Count > 1)
                        {
                            isAmbigous = true;
                        }

                        foreach (Location location in googleResult.Locations)
                        {

                            if (location != null)
                            {

                                Geocode geocode = new Geocode(Convert.ToDouble(query.BaseOptions.Version.ToString()));

                                geocode.InputAddress = query.StreetAddress;
                                geocode.ParsedAddress = query.StreetAddress;
                                geocode.SetMatchedLocationType(MatchedLocationTypes.StreetAddress);

                                StreetAddress returnedAddress = null;

                                if (Convert.ToDouble(query.BaseOptions.Version.ToString()) >= 2.95)
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


                                    AddressNormalizer addressNormalizerStreetAddress = new AddressNormalizer(AddressParserType.TokenBased, addressComponentsStreetAddress, Convert.ToDouble(query.BaseOptions.Version.ToString()), AddressFormatType.LACounty);
                                    returnedAddress = addressNormalizerStreetAddress.Normalize(location.Address.Street);
                                }
                                else
                                {
                                    AddressNormalizer addressNormalizer = new AddressNormalizer(AddressParserType.TokenBased, Convert.ToDouble(query.BaseOptions.Version.ToString()), AddressFormatType.LACounty);
                                    returnedAddress = addressNormalizer.NormalizeStreetAddress(location.Address.Street);
                                }

                                //StreetAddress returnedAddress = new StreetAddress();
                                //returnedAddress.StreetName = location.Address.Street;
                                
                                geocode.MatchedAddress = RelaxableStreetAddress.FromStreetAddress(returnedAddress);
                                geocode.MatchedAddress.City = location.Address.City;
                                geocode.MatchedAddress.State = location.Address.Region;
                                geocode.MatchedAddress.ZIP = location.Address.Zip;
                                
                                geocode.MatchedFeatureAddress = RelaxableStreetAddress.FromStreetAddress(returnedAddress);
                                geocode.MatchedFeatureAddress.City = location.Address.City;
                                geocode.MatchedFeatureAddress.State = location.Address.Region;
                                geocode.MatchedFeatureAddress.ZIP = location.Address.Zip;

                                geocode.Geometry = new Point(location.Point.Longitude, location.Point.Latitude);


                                if (isAmbigous)
                                {
                                    geocode.FM_ResultType = FeatureMatchingResultType.Ambiguous;
                                }
                                else
                                {
                                    geocode.FM_ResultType = FeatureMatchingResultType.Success;
                                }


                                switch (location.Accuracy)
                                {
                                    case GeocodeAccuracy.Premise:
                                        geocode.FM_GeographyType = FeatureMatchingGeographyType.BuildingCentroid;
                                        geocode.GeocodeQualityType = GeocodeQualityType.BuildingCentroid;
                                        break;
                                    case GeocodeAccuracy.Address:
                                        geocode.FM_GeographyType = FeatureMatchingGeographyType.StreetSegment;
                                        geocode.GeocodeQualityType = GeocodeQualityType.AddressRangeInterpolation;
                                        break;
                                    case GeocodeAccuracy.Country:
                                        geocode.FM_GeographyType = FeatureMatchingGeographyType.Country;
                                        geocode.GeocodeQualityType = GeocodeQualityType.CountryCentroid;
                                        break;
                                    case GeocodeAccuracy.Intersection:
                                        geocode.FM_GeographyType = FeatureMatchingGeographyType.StreetIntersection;
                                        geocode.GeocodeQualityType = GeocodeQualityType.StreetIntersection;
                                        break;
                                    case GeocodeAccuracy.PostCode:
                                        geocode.FM_GeographyType = FeatureMatchingGeographyType.USPSZip;
                                        geocode.GeocodeQualityType = GeocodeQualityType.ZCTACentroid;
                                        break;
                                    case GeocodeAccuracy.Region:
                                        geocode.FM_GeographyType = FeatureMatchingGeographyType.State;
                                        geocode.GeocodeQualityType = GeocodeQualityType.StateCentroid;
                                        break;
                                    case GeocodeAccuracy.Street:
                                        geocode.FM_GeographyType = FeatureMatchingGeographyType.StreetCentroid;
                                        geocode.GeocodeQualityType = GeocodeQualityType.StreetCentroid;
                                        break;
                                    case GeocodeAccuracy.SubRegion:
                                        geocode.FM_GeographyType = FeatureMatchingGeographyType.CountySubRegion;
                                        geocode.GeocodeQualityType = GeocodeQualityType.CountySubdivisionCentroid;
                                        break;
                                    case GeocodeAccuracy.Town:
                                        geocode.FM_GeographyType = FeatureMatchingGeographyType.City;
                                        geocode.GeocodeQualityType = GeocodeQualityType.CityCentroid;
                                        break;
                                    case GeocodeAccuracy.UnknownLocation:
                                        geocode.FM_GeographyType = FeatureMatchingGeographyType.Unknown;
                                        geocode.GeocodeQualityType = GeocodeQualityType.Unknown;
                                        break;
                                    default:
                                        throw new Exception("Unknown or unexpected Google Accuracy type" + location.Accuracy);
                                }

                                DateTime endTime = DateTime.Now;

                                TimeSpan timeTaken = endTime.Subtract(startTime);

                                geocode.TotalTimeTaken = timeTaken;

                                ret.AddGeocode(geocode);
                            }
                        }
                    }
                }
                else if (googleResult.Status == GeocodeStatus.G_GEO_MISSING_ADDRESS || googleResult.Status == GeocodeStatus.G_GEO_UNAVAILABLE_ADDRESS || googleResult.Status == GeocodeStatus.G_GEO_UNKNOWN_ADDRESS )
                {
                    Geocode geocode = new Geocode(Convert.ToDouble(query.BaseOptions.Version.ToString()));
                    geocode.FM_ResultType = FeatureMatchingResultType.Unmatchable;
                    geocode.InputAddress = query.StreetAddress;
                    geocode.ParsedAddress = query.StreetAddress;
                    geocode.ErrorMessage = googleResult.Status.ToString();

                    DateTime endTime = DateTime.Now;

                    TimeSpan timeTaken = endTime.Subtract(startTime);

                    geocode.TotalTimeTaken = timeTaken;

                    ret.AddGeocode(geocode);
                }
                else
                {
                    Geocode geocode = new Geocode(Convert.ToDouble(query.BaseOptions.Version.ToString()));
                    geocode.FM_ResultType = FeatureMatchingResultType.ExceptionOccurred;
                    geocode.InputAddress = query.StreetAddress;
                    geocode.ParsedAddress = query.StreetAddress;
                    geocode.ErrorMessage = googleResult.Status.ToString();

                    DateTime endTime = DateTime.Now;

                    TimeSpan timeTaken = endTime.Subtract(startTime);

                    geocode.TotalTimeTaken = timeTaken;

                    ret.AddGeocode(geocode);
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
