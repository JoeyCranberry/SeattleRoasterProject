using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;

namespace RoasterBeansDataAccess.Models
{
    public class BeanModel
    {
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string Id { get; set; }
        public string FullName { get; set; }
        public int RoasterId { get; set; }
        public string MongoRoasterId { get; set; }
        public DateTime DateAdded { get; set; }
        public string ProductURL { get; set; }
        public string ImageURL { get; set; }
        public string ImageClass { get; set; }
        public decimal PriceBeforeShipping { get; set; }
        public BeanProcessing ProcessingMethod { get; set; }
        public RoastLevel RoastLevel { get; set; }
        public List<Country> CountriesOfOrigin { get; set; }
        public List<Region> RegionsOfOrigin { get; set; }
        public bool IsFairTradeCertified { get; set; }
        public bool IsDirectTradeCertified { get; set; }
        public OrganicCerification OrganicCerification { get; set; }
        public List<string> TastingNotes { get; set; }
        public bool IsSingleOrigin { get; set; }
        public bool IsDecaf { get; set; }
        public bool IsExcluded { get; set; } = false;
        public bool AvailablePreground { get; set; } = false;
        public bool InStock { get; set; } = true;
        public decimal SizeOunces { get; set; } = 0;
        public bool IsFromWomanOwnedFarms { get; set; } = false;
        public bool IsSupportingCause { get; set; } = false;
        public bool IsRainforestAllianceCertified { get; set; } = false;
		public string SupportedCause { get; set; }

		#region Processing
		public void SetOriginsFromName()
        {
            List<Country> countriesFromName = new List<Country>();

            foreach (var country in Enum.GetValues<Country>())
            {
                if (FullName.ToLower().Contains(country.ToString().ToLower().Replace("_", " ")))
                {
                    countriesFromName.Add(country);
                }
            }

            if (FullName.ToLower().Contains("sumatra"))
            {
                countriesFromName.Add(Country.INDONESIA);
            }

            if (countriesFromName.Count > 0)
            {
                CountriesOfOrigin = countriesFromName;

                if (countriesFromName.Count == 1 && countriesFromName[0] != Country.UNKNOWN)
                {
                    IsSingleOrigin = true;
                }
                else
                {
                    IsSingleOrigin = false;
                }
            }

            if (FullName.ToLower().Contains("blend"))
            {
                IsSingleOrigin = false;
            }
        }

        public void SetProcessFromName()
        {
            foreach (var process in Enum.GetValues<BeanProcessing>())
            {
                if (FullName.ToLower().Contains(process.ToString().ToLower().Replace("_", " ")))
                {
                    ProcessingMethod = process;
                }
            }
        }

        public void SetDecafFromName()
        {
            if (FullName.ToLower().Contains(value: "decaf"))
            {
                IsDecaf = true;
            }
        }

        public void SetOrganicFromName()
        {
            if (FullName.ToLower().Contains(value: "organic"))
            {
                if (FullName.ToLower().Contains(value: "usda"))
                {
                    OrganicCerification = OrganicCerification.CERTIFIED_ORGANIC;
                }
                else
                {
                    OrganicCerification = OrganicCerification.UNCERTIFIED_ORGANIC;
                }
            }
        }

        public void SetRoastLevelFromName()
        {
            foreach (var roast in Enum.GetValues<RoastLevel>())
            {
                if (FullName.ToLower().Contains(roast.ToString().ToLower().Replace("_", " ")))
                {
                    RoastLevel = roast;
                }
            }
        }

		public void SetFairTradeFromName()
		{
			if (FullName.ToLower().Contains(value: "fair-trade") || FullName.ToLower().Contains(value: "fair trade"))
			{
                IsFairTradeCertified = true;
			}
		}

		#endregion

		#region Static Helpers
		// Converts the Country enum into title case and optionally adds flag emoji
		public static string GetCountryDisplayName(Country country, bool includeFlag = false)
        {
            string titleCase = GetTitleCase(country.ToString());

			if (includeFlag)
            {
                switch(country)
                {
                    case Country.ETHIOPIA:
                        titleCase = "🇪🇹 " + titleCase;
                        break;
                    case Country.COLOMBIA:
                        titleCase = "🇨🇴 " + titleCase;
                        break;
                    case Country.RWANDA:
                        titleCase = "🇪🇹 " + titleCase;
                        break;
                    case Country.GUATEMALA:
                        titleCase = "🇷🇼 " + titleCase;
                        break;
                    case Country.EL_SALVADOR:
                        titleCase = "🇸🇻 " + titleCase;
                        break;
                    case Country.INDONESIA:
                        titleCase = "🇮🇩 " + titleCase;
                        break;
                    case Country.HONDURAS:
                        titleCase = "🇭🇳 " + titleCase;
                        break;
                    case Country.NICARAGUA:
                        titleCase = "🇳🇮 " + titleCase;
                        break;
                    case Country.BRAZIL:
                        titleCase = "🇧🇷 " + titleCase;
                        break;
                    case Country.KENYA:
                        titleCase = "🇰🇪 " + titleCase;
                        break;
                    case Country.MEXICO:
						titleCase = "🇲🇽 " + titleCase;
						break;
					case Country.COSTA_RICA:
						titleCase = "🇨🇷 " + titleCase;
						break;
                    case Country.PAPAU_NEW_GUINEA:
                        titleCase = "🇵🇬 " + titleCase;
                        break;
                    case Country.PERU:
						titleCase = "🇵🇪 " + titleCase;
						break;
					case Country.UGANDA:
						titleCase = "🇺🇬 " + titleCase;
						break;
					case Country.BURUNDI:
						titleCase = "🇧🇮 " + titleCase;
						break;
					case Country.DEMOCRATIC_REPUBLIC_OF_THE_CONGO:
                        titleCase = "🇨🇩 DR Congo";
						break;
					case Country.TANZANIA:
						titleCase = "🇹🇿 " + titleCase;
						break;
                    case Country.EAST_TIMOR:
						titleCase = "🇹🇱 " + titleCase;
						break;
				}
            }
            
            return titleCase;
        }

        public static string GetRegionDisplayName(Region region)
        {
            return GetTitleCase(region.ToString());
		}

		public static string GetCountryDemonym(Country country)
        {
			switch (country)
			{
				case Country.ETHIOPIA:
                    return "Ethiopian";
				case Country.COLOMBIA:
					return "Columbian";
				case Country.RWANDA:
					return "Rwandan";
				case Country.GUATEMALA:
					return "Guatemalian";
				case Country.EL_SALVADOR:
					return "El Salvadorian";
				case Country.INDONESIA:
					return "Indonesian";
				case Country.HONDURAS:
					return "Honduran";
				case Country.NICARAGUA:
					return "Nicaraguan";
				case Country.BRAZIL:
					return "Brazilian";
				case Country.KENYA:
					return "Kenyan";
                case Country.MEXICO:
                    return "Mexican";
				case Country.COSTA_RICA:
					return "Costa Rican";
				case Country.PAPAU_NEW_GUINEA:
					return "Papua New Guinean";
                case Country.PERU:
                    return "Peruvian";
                case Country.UGANDA:
                    return "Ugandan";
				case Country.BURUNDI:
					return "Umurundi";
                case Country.DEMOCRATIC_REPUBLIC_OF_THE_CONGO:
                    return "Congolese";
                case Country.TANZANIA:
                    return "Tanzanian";
                case Country.EAST_TIMOR:
                    return "Timorese";
				default:
                    return country.ToString();
			}
		}

        public static string GetProcessDisplayName(BeanProcessing process)
        {
            return GetTitleCase(process.ToString());
		}

		public static string GetRoastDisplayName(RoastLevel roast)
		{
			return GetTitleCase(roast.ToString());
		}

		public static string GetOrganicCertificationDisplayName(OrganicCerification organic)
		{
			return GetTitleCase(organic.ToString());
		}

		public static int GetRoastOrder(RoastLevel roast)
        {
            switch(roast)
            {
                case RoastLevel.UNKNOWN:
                    return 0;
                case RoastLevel.GREEN: 
                    return 1;
                case RoastLevel.LIGHT:
                    return 2;
                case RoastLevel.MEDIUM: 
                    return 3;
                default:
                case RoastLevel.DARK:
                    return 4;
            }
        }

		private static string GetTitleCase(string input)
        {
			TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
			return textInfo.ToTitleCase(input.ToLower().Replace("_", " "));
		}

		#endregion

		#region Property Accessors

		public decimal? GetPricePerOz()
        {
            if(PriceBeforeShipping != 0 && SizeOunces != 0)
            {
                return PriceBeforeShipping / SizeOunces;
			}
            else
            {
                return null;
            }
        }

        public string GetPricePerOzString()
        {
            decimal? pricePerOz = GetPricePerOz();
            if(pricePerOz != null)
            {
                return "($" + pricePerOz.Value.ToString("0.00") + "/oz)";
            }
            else
            {
                return "";
            }
		}

        public string GetDisplayRoastLevel()
        {
			return GetTitleCase(RoastLevel.ToString());
		}

        public string GetQuickProperties()
        {
			TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;

			List<string> properties = new List<string>();

            if(IsSingleOrigin)
            {
                properties.Add("Single Origin");
			}
            else
            {
				properties.Add("Blend");
			}

            if(ProcessingMethod != BeanProcessing.UNKNOWN)
            {
				properties.Add(GetProcessDisplayName(ProcessingMethod));
			}

            if(RoastLevel != RoastLevel.UNKNOWN)
            {
                properties.Add(GetDisplayRoastLevel());
			}

            return String.Join("<i class=\"bi bi-circle px-2 icon-small \"></i>", properties);
        }

		#endregion
	}

	public enum BeanProcessing
    {
        UNKNOWN,
        NATURAL,
        HONEY,
        WASHED,
        WET_HULLED
    }

    public enum RoastLevel
    {
        UNKNOWN,
        LIGHT,
        MEDIUM,
        DARK,
        GREEN
    }

    public enum OrganicCerification
    {
        NOT_ORGANIC,
        CERTIFIED_ORGANIC,
        UNCERTIFIED_ORGANIC
    }

    public enum Country
    {
        UNKNOWN,
        ETHIOPIA,
        COLOMBIA,
        RWANDA,
        GUATEMALA,
        EL_SALVADOR,
        INDONESIA,
        HONDURAS,
        NICARAGUA,
        BRAZIL,
        KENYA,
        MEXICO,
        COSTA_RICA,
        PAPAU_NEW_GUINEA,
        PERU,
        UGANDA,
		BURUNDI,
        DEMOCRATIC_REPUBLIC_OF_THE_CONGO,
        TANZANIA,
        EAST_TIMOR
	}

    public enum Region
    {
        CENTRAL_AMERICA,
        SOUTH_AMERICA,
        AFRICA,
        ASIA
    }
}
