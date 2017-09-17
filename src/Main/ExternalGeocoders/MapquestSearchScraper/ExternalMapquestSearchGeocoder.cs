using System;
using USC.GISResearchLab.AddressProcessing.Core.AddressNormalization.Implementations;
using USC.GISResearchLab.AddressProcessing.Core.Parsing.AddressParserManagers.Factories;
using USC.GISResearchLab.Common.Addresses;
using USC.GISResearchLab.Common.Core.Geocoders.GeocodingQueries;
using USC.GISResearchLab.Common.Geometries.Points;
using USC.GISResearchLab.Geocoding.Core.Configurations;
using USC.GISResearchLab.Geocoding.Core.Metadata.FeatureMatchingResults;
using USC.GISResearchLab.Geocoding.Core.Metadata.Qualities;
using USC.GISResearchLab.Geocoding.Core.OutputData;

namespace USC.GISResearchLab.Geocoding.Core.ExternalGeocoders.MapquestSearchScraper
{
    public class ExternalMapquestSearchGeocoder : AbstractSingleThreadedGeocoder, ICloneable
    {
        #region Properties

        public bool ShouldDoExhaustiveSearch { get; set; }

        #endregion

        public ExternalMapquestSearchGeocoder() : base() { }

        public override GeocodeResultSet Geocode(GeocodingQuery query, GeocoderConfiguration geocoderConfiguration)
        {

            GeocodeResultSet ret = new GeocodeResultSet();
            ret.StartTime = DateTime.Now;
            try
            {
                //Address address = new Address();
                //address.AdministrativeArea = query.StreetAddress.State;
                //address.AdministrativeArea = query.StreetAddress.State;
                //address.City = query.StreetAddress.City;
                //address.Country = "US";
                //address.Street = query.StreetAddress.GetStreetAddressPortionAsString();

                DateTime startTime = DateTime.Now;
                MapquestSearchScraperAddress result = MapquestSearchAPI.Geocode(query.StreetAddress.OtherInformation, query.StreetAddress.GetStreetAddressPortionAsString(), query.StreetAddress.City, query.StreetAddress.State, query.StreetAddress.ZIP, "");

                if (result != null)
                {


                    Geocode geocode = new Geocode(Convert.ToDouble(query.BaseOptions.Version.ToString()));

                    if (!String.IsNullOrEmpty(result.ErrorMessage))
                    {
                        geocode.ErrorMessage = result.ErrorMessage;
                        geocode.ExceptionOccurred = true;
                    }

                    geocode.InputAddress = query.StreetAddress;
                    geocode.ParsedAddress = query.StreetAddress;


                    geocode.FM_ResultType = FeatureMatchingResultType.Success;


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

                        if (!String.IsNullOrEmpty(result.Name))
                        {
                            returnedAddress.Name = result.Name;
                        }

                        if (!String.IsNullOrEmpty(result.Phone))
                        {
                            returnedAddress.Phone = result.Phone;
                        }

                        if (!String.IsNullOrEmpty(result.Id))
                        {
                            returnedAddress.Id = result.Id;
                        }
                    }
                    else
                    {
                        AddressNormalizer addressNormalizer = new AddressNormalizer(AddressParserType.TokenBased, Convert.ToDouble(geocoderConfiguration.Version.ToString()), AddressFormatType.LACounty);
                        returnedAddress = addressNormalizer.NormalizeStreetAddress(result.Street, result.City, result.StateCode, result.ZipCode);

                    }

                    geocode.MatchedAddress = RelaxableStreetAddress.FromStreetAddress(returnedAddress);
                    
                    if (String.IsNullOrEmpty(geocode.MatchedAddress.City))
                    {
                        geocode.MatchedAddress.City = result.City;
                    }

                    if (String.IsNullOrEmpty(geocode.MatchedAddress.StateCode))
                    {
                        geocode.MatchedAddress.State = result.StateCode;
                    }

                    if (String.IsNullOrEmpty(geocode.MatchedAddress.ZIP))
                    {
                        geocode.MatchedAddress.ZIP = result.ZipCode;
                    }

                    geocode.Geometry = new Point(Convert.ToDouble(result.Longitude), Convert.ToDouble(result.Latitude));




                    geocode.FM_GeographyType = FeatureMatchingGeographyType.StreetSegment;


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
