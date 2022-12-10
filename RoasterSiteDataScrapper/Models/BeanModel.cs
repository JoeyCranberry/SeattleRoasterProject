using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Globalization;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;
using PuppeteerSharp.Input;
using static System.Formats.Asn1.AsnWriter;
using RoasterBeansDataAccess.Services;

namespace RoasterBeansDataAccess.Models
{
	public class BeanModel
	{
		// Basic Information
		[BsonId]
		[BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
		public string Id { get; set; }
		public string FullName { get; set; }
		public int RoasterId { get; set; }
		public string MongoRoasterId { get; set; }
		public DateTime DateAdded { get; set; }

		// Linking
		public string ProductURL { get; set; }
		public string ImageURL { get; set; }
		public string ImageClass { get; set; }

		// Pricing and size
		public decimal PriceBeforeShipping { get; set; }
		public decimal SizeOunces { get; set; } = 0;

		// Sourcing and Characteristics
		public List<ProccessingMethod> ProcessingMethods { get; set; }
		public RoastLevel RoastLevel { get; set; }
		public List<SourceOrigin> Origins { get; set; }
		public bool IsSingleOrigin { get; set; }
		public bool IsDecaf { get; set; }

		// Sourcing Scoring
		public bool HasProducerInfo { get; set; } = false;
		public bool HasImporterName { get; set; } = false;
		public bool HasProcessorName { get; set; } = false;

		// Certifications
		public bool IsFairTradeCertified { get; set; }
		public bool IsDirectTradeCertified { get; set; }
		public bool IsAboveFairTradePricing { get; set; } = false;
		public bool IsRainforestAllianceCertified { get; set; } = false;
		public OrganicCerification OrganicCerification { get; set; }

		// Listing Fields
		public bool IsExcluded { get; set; } = false;
		public bool AvailablePreground { get; set; } = false;
		public bool InStock { get; set; } = true;
		public bool IsProductionVisible = true;

		// Social Causes
		public bool IsFromWomanOwnedFarms { get; set; } = false;
		public bool IsSupportingCause { get; set; } = false;
		public string SupportedCause { get; set; }

		// Roaster notes
		public List<BrewMethod> RecommendedBrewMethods { get; set; }
		public List<string> TastingNotes { get; set; }

		#region Processing
		public void SetOriginsFromName()
		{
			List<SourceCountry> countriesFromName = new List<SourceCountry>();

			foreach (var country in Enum.GetValues<SourceCountry>())
			{
				if (FullName.ToLower().Contains(country.ToString().ToLower().Replace("_", " ")))
				{
					countriesFromName.Add(country);
				}
			}

			if (FullName.ToLower().Contains("sumatra") && !countriesFromName.Contains(SourceCountry.INDONESIA))
			{
				countriesFromName.Add(SourceCountry.INDONESIA);
			}

			if (FullName.ToLower().Contains("congo") && !countriesFromName.Contains(SourceCountry.DEMOCRATIC_REPUBLIC_OF_THE_CONGO))
			{
				countriesFromName.Add(SourceCountry.DEMOCRATIC_REPUBLIC_OF_THE_CONGO);
			}

			if (countriesFromName.Count > 0)
			{
				Origins = new();
				foreach (SourceCountry country in countriesFromName)
				{
					Origins.Add(new SourceOrigin(country));
				}

				if (countriesFromName.Count == 1 && countriesFromName[0] != SourceCountry.UNKNOWN)
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
			foreach (var process in Enum.GetValues<ProccessingMethod>())
			{
				if (FullName.ToLower().Contains(process.ToString().ToLower().Replace("_", " ")))
				{
					if (ProcessingMethods == null)
					{
						ProcessingMethods = new List<ProccessingMethod>();
					}

					ProcessingMethods.Add(process);
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
			if (FullName.ToLower().Contains(value: "organic") || FullName.ToLower().Contains(value: "fto"))
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
			if (FullName.ToLower().Contains(value: "fair-trade") || FullName.ToLower().Contains(value: "fair trade") || FullName.ToLower().Contains(value: "fto"))
			{
				IsFairTradeCertified = true;
			}
		}

		#endregion

		#region Static Helpers
		// Converts the Country enum into title case and optionally adds flag emoji
		public static string GetCountryDisplayName(SourceCountry country, bool includeFlag = false)
		{
			string titleCase = GetTitleCase(country.ToString());

			switch (country)
			{
				case SourceCountry.DEMOCRATIC_REPUBLIC_OF_THE_CONGO:
					titleCase = "DR Congo";
					break;
			}

			if (includeFlag)
			{
				string countryFlag = GetCountryFlag(country);

				return countryFlag + " " + titleCase;
			}

			return titleCase;
		}

		private static string GetCountryFlag(SourceCountry country)
		{
			switch (country)
			{
				case SourceCountry.ETHIOPIA:
					return "🇪🇹";
				case SourceCountry.COLOMBIA:
					return "🇨🇴";
				case SourceCountry.RWANDA:
					return "🇷🇼";
				case SourceCountry.GUATEMALA:
					return "🇬🇹";
				case SourceCountry.EL_SALVADOR:
					return "🇸🇻";
				case SourceCountry.INDONESIA:
					return "🇮🇩";
				case SourceCountry.HONDURAS:
					return "🇭🇳";
				case SourceCountry.NICARAGUA:
					return "🇳🇮";
				case SourceCountry.BRAZIL:
					return "🇧🇷";
				case SourceCountry.KENYA:
					return "🇰🇪";
				case SourceCountry.MEXICO:
					return "🇲🇽";
				case SourceCountry.COSTA_RICA:
					return "🇨🇷";
				case SourceCountry.PAPAU_NEW_GUINEA:
					return "🇵🇬";
				case SourceCountry.PERU:
					return "🇵🇪";
				case SourceCountry.UGANDA:
					return "🇺🇬";
				case SourceCountry.BURUNDI:
					return "🇧🇮";
				case SourceCountry.DEMOCRATIC_REPUBLIC_OF_THE_CONGO:
					return "🇨🇩";
				case SourceCountry.TANZANIA:
					return "🇹🇿";
				case SourceCountry.EAST_TIMOR:
					return "🇹🇱";
				case SourceCountry.DOMINICAN_REPUBLIC:
					return "🇩🇴";
				case SourceCountry.VIETNAM:
					return "🇻🇳";
				case SourceCountry.ECUADOR:
					return "🇪🇨";
				case SourceCountry.CHINA:
					return "🇨🇳";
				case SourceCountry.MYANMAR:
					return "🇲🇲";
				case SourceCountry.THAILAND:
					return "🇹🇭";
				case SourceCountry.HAITI:
					return "🇭🇹";
				case SourceCountry.YEMEN:
					return "🇾🇪";
				default:
					return "🌎";
			}
		}

		public static string GetContinentDisplayName(SourceContinent region)
		{
			string worldString = "";
			switch (region)
			{
				case SourceContinent.SOUTH_AMERICA:
				case SourceContinent.CENTRAL_AMERICA:
					worldString = "🌎 ";
					break;
				case SourceContinent.AFRICA:
					worldString = "🌍 ";
					break;
				case SourceContinent.ASIA:
					worldString = "🌏 ";
					break;
			}

			return worldString + GetTitleCase(region.ToString());
		}

		public static string GetCountryDemonym(SourceCountry country)
		{
			switch (country)
			{
				case SourceCountry.ETHIOPIA:
					return "Ethiopian";
				case SourceCountry.COLOMBIA:
					return "Colombian";
				case SourceCountry.RWANDA:
					return "Rwandan";
				case SourceCountry.GUATEMALA:
					return "Guatemalian";
				case SourceCountry.EL_SALVADOR:
					return "El Salvadorian";
				case SourceCountry.INDONESIA:
					return "Indonesian";
				case SourceCountry.HONDURAS:
					return "Honduran";
				case SourceCountry.NICARAGUA:
					return "Nicaraguan";
				case SourceCountry.BRAZIL:
					return "Brazilian";
				case SourceCountry.KENYA:
					return "Kenyan";
				case SourceCountry.MEXICO:
					return "Mexican";
				case SourceCountry.COSTA_RICA:
					return "Costa Rican";
				case SourceCountry.PAPAU_NEW_GUINEA:
					return "Papua New Guinean";
				case SourceCountry.PERU:
					return "Peruvian";
				case SourceCountry.UGANDA:
					return "Ugandan";
				case SourceCountry.BURUNDI:
					return "Umurundi";
				case SourceCountry.DEMOCRATIC_REPUBLIC_OF_THE_CONGO:
					return "Congolese";
				case SourceCountry.TANZANIA:
					return "Tanzanian";
				case SourceCountry.EAST_TIMOR:
					return "Timorese";
				case SourceCountry.DOMINICAN_REPUBLIC:
					return "Dominican";
				case SourceCountry.VIETNAM:
					return "Vietnamese";
				case SourceCountry.ECUADOR:
					return "Ecuadorian";
				case SourceCountry.CHINA:
					return "Chinese";
				case SourceCountry.MYANMAR:
					return "Burmese";
				case SourceCountry.THAILAND:
					return "Thai";
				case SourceCountry.HAITI:
					return "Haitian";
				case SourceCountry.YEMEN:
					return "Yemeni";
				default:
					return country.ToString();
			}
		}

		public static string GetOriginLongDisplay(SourceOrigin origin, bool includeFlag = false)
		{
			string originLongName = "";

			if(includeFlag)
			{
				if(origin.Country != SourceCountry.UNKNOWN)
				{
					originLongName += GetCountryFlag(origin.Country) + " ";
				}
				else if(origin.Continent != null)
				{
					return GetContinentDisplayName(origin.Continent.Value);
				}
			}


			if(!String.IsNullOrEmpty(origin.Region))
			{
				originLongName += origin.Region + ", ";
			}
			else if (!String.IsNullOrEmpty(origin.City))
			{
				originLongName += origin.City + ", ";
			}

			if (origin.Country != SourceCountry.UNKNOWN)
			{
				originLongName += GetCountryDisplayName(origin.Country);
			}

			return originLongName;
		}

		public static string GetProcessDisplayName(ProccessingMethod process)
		{
			return GetTitleCase(process.ToString());
		}

		public static string GetBrewMethodDisplayName(BrewMethod method)
		{
			return GetTitleCase(method.ToString());
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
			switch (roast)
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
			if (PriceBeforeShipping != 0 && SizeOunces != 0)
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
			if (pricePerOz != null)
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

			if (IsSingleOrigin)
			{
				properties.Add("Single Origin");
			}
			else
			{
				properties.Add("Blend");
			}

			if (ProcessingMethods != null && ProcessingMethods.Count > 0)
			{
				List<string> processingMethods = new List<string>();
				foreach (var process in ProcessingMethods)
				{
					processingMethods.Add(GetProcessDisplayName(process));
				}
				properties.Add(String.Join("/", processingMethods));
			}

			if (RoastLevel != RoastLevel.UNKNOWN)
			{
				properties.Add(GetDisplayRoastLevel());
			}

			return String.Join("<i class=\"bi bi-circle px-2 icon-small \"></i>", properties);
		}

		public string GetAllRegionsAndCities()
		{
			string returnString = "";
			if(Origins != null)
			{
				foreach (SourceOrigin origin in Origins)
				{
					if (!String.IsNullOrEmpty(origin.City))
					{
						returnString += origin.City + ", ";
					}

					if (!String.IsNullOrEmpty(origin.Region))
					{
						returnString += origin.Region + " ";
					}
				}
			}

			return returnString;
		}

		public int GetTraceabilityScore()
		{
			return TraceabilityService.GetTotalScore(this);
		}

		public string GetTraceabilityScoreStarDisplay()
		{
			return TraceabilityService.GetScoreStarDisplay(this);
		}

		public string GetTraceabilityScoreBreakdownDisplay()
		{
			return TraceabilityService.GetScoreBreakdownDisplay(this);
		}

		public List<SourceCountry> GetOriginCountries()
		{
			List<SourceCountry> countries = new();
			if (Origins != null)
			{
				foreach (var origin in Origins)
				{
					if (origin.Country != SourceCountry.UNKNOWN)
					{
						countries.Add(origin.Country);
					}
				}
			}

			return countries;
		}

		#endregion
	}

	public enum ProccessingMethod
	{
		UNKNOWN,
		NATURAL,
		HONEY,
		WASHED,
		WET_HULLED,
		SWISS_WATER,
		SUGARCANE_DECAF,
		LACTIC,
		ANAEROBIC
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

	public enum SourceCountry
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
		EAST_TIMOR,
		DOMINICAN_REPUBLIC,
		VIETNAM,
		ECUADOR,
		CHINA,
		MYANMAR,
		THAILAND,
		HAITI,
		YEMEN
	}

	public enum SourceContinent
	{
		CENTRAL_AMERICA,
		SOUTH_AMERICA,
		AFRICA,
		ASIA
	}

	public class SourceOrigin
	{
		public string? City { get; set; }
		public string? Region { get; set; }
		public SourceCountry Country { get; set; } = SourceCountry.UNKNOWN;
		public SourceContinent? Continent { get; set; }

		public SourceOrigin()
		{

		}

		public SourceOrigin(string? city, string? region, SourceCountry country)
		{
			City = city;
			Region = region;
			Country = country;
			Continent = GetContinentFromCountry(country);
		}

		public SourceOrigin(SourceCountry country)
		{
			Country = country;
			Continent = GetContinentFromCountry(country);
		}

		public SourceOrigin(SourceContinent continent)
		{
			Continent = continent;
		}

		public static SourceContinent? GetContinentFromCountry(SourceCountry country)
		{
			switch (country)
			{
				case SourceCountry.ETHIOPIA:
				case SourceCountry.RWANDA:
				case SourceCountry.KENYA:
				case SourceCountry.UGANDA:
				case SourceCountry.BURUNDI:
				case SourceCountry.DEMOCRATIC_REPUBLIC_OF_THE_CONGO:
				case SourceCountry.TANZANIA:
					return SourceContinent.AFRICA;
				case SourceCountry.COLOMBIA:
				case SourceCountry.BRAZIL:
				case SourceCountry.PERU:
				case SourceCountry.ECUADOR:
					return SourceContinent.SOUTH_AMERICA;
				case SourceCountry.GUATEMALA:
				case SourceCountry.EL_SALVADOR:
				case SourceCountry.HONDURAS:
				case SourceCountry.NICARAGUA:
				case SourceCountry.MEXICO:
				case SourceCountry.COSTA_RICA:
				case SourceCountry.DOMINICAN_REPUBLIC:
				case SourceCountry.HAITI:
					return SourceContinent.CENTRAL_AMERICA;
				case SourceCountry.INDONESIA:
				case SourceCountry.PAPAU_NEW_GUINEA:
				case SourceCountry.EAST_TIMOR:
				case SourceCountry.VIETNAM:
				case SourceCountry.CHINA:
				case SourceCountry.MYANMAR:
				case SourceCountry.THAILAND:
					return SourceContinent.ASIA;
			}

			return null;
		}
	}

	public enum BrewMethod
	{
		POUR_OVER,
		IMMERSION,
		ESPRESSO,
		COLD_BREW,
		MOKA_POT,
		DRIP,
		FRENCH_PRESS,
		AERO_PRESS
	}

	public enum BrewCategory
	{
		ESPRESSO,
		FILTER,
		COLD_BREW
	}
}
