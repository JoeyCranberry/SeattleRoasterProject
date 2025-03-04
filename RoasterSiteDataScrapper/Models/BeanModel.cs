using System.Globalization;
using MongoDB.Bson.Serialization.Attributes;
using RoasterBeansDataAccess.Services;
using SeattleRoasterProject.Core.Enums;
using static RoasterBeansDataAccess.Models.BeanOrigin;

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
		public List<ProcessingMethod>? ProcessingMethods { get; set; }
		public RoastLevel RoastLevel { get; set; }
		public List<SourceLocation>? Origins { get; set; }
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
		public OrganicCertification OrganicCertification { get; set; }

		// Listing Fields
		public bool IsExcluded { get; set; } = false;
		public bool AvailablePreground { get; set; } = false;
		public bool InStock { get; set; } = true;
		public bool IsProductionVisible = true;
		public bool? IsActiveListing = true;

		// Social Causes
		public bool IsFromWomanOwnedFarms { get; set; } = false;
		public bool IsSupportingCause { get; set; } = false;
		public string SupportedCause { get; set; } = string.Empty;

		// Roaster notes
		public List<BrewMethod>? RecommendedBrewMethods { get; set; }
        public List<string>? TastingNotes { get; set; } = new();

		#region Processing
		public void SetOriginsFromName()
		{
			List<SourceCountry> countriesFromName = BeanNameParsing.GetCountriesFromName(FullName);

			if (countriesFromName.Count > 0)
			{
				Origins = new();
				foreach (SourceCountry country in countriesFromName)
				{
					Origins.Add(new SourceLocation(country));
				}

				if (countriesFromName.Count == 1 && countriesFromName[0] != SourceCountry.Unknown)
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
			ProcessingMethods = BeanNameParsing.GetProcessFromName(FullName);
		}

		public void SetDecafFromName()
		{
			IsDecaf = BeanNameParsing.GetIsDecafFromName(FullName);
		}

		public void SetOrganicFromName()
		{
			OrganicCertification = BeanNameParsing.GetOrganicFromName(FullName);
		}

		public void SetRoastLevelFromName()
		{
			RoastLevel = BeanNameParsing.SetRoastLevelFromName(FullName);
		}

		public void SetFairTradeFromName()
		{
			IsFairTradeCertified = BeanNameParsing.SetIsFairTradeFromName(FullName);
		}

		#endregion

		#region Static Helpers
		

		public static string GetProcessDisplayName(ProcessingMethod process)
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

		public static string GetOrganicCertificationDisplayName(OrganicCertification organic)
		{
			return GetTitleCase(organic.ToString());
		}

		public static int GetRoastOrder(RoastLevel roast)
		{
			return roast switch
			{
				RoastLevel.Unknown => 0,
				RoastLevel.Green => 1,
				RoastLevel.Light => 2,
				RoastLevel.Medium => 3,
				RoastLevel.Dark => 4,
				_ => throw new ArgumentOutOfRangeException(nameof(roast), $"Not expected roast value: {roast}")
			};
		}

		public static string GetTitleCase(string input)
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

		public List<string> GetQuickProperties()
		{
			List<string> properties = new();

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
				List<string> processingMethods = new();
				foreach (var process in ProcessingMethods)
				{
					processingMethods.Add(GetProcessDisplayName(process));
				}
				properties.Add(String.Join("/", processingMethods));
			}

			if (RoastLevel != RoastLevel.Unknown)
			{
				properties.Add(GetDisplayRoastLevel());
			}

			return properties;
		}

		public string GetAllRegionsAndCities()
		{
			string returnString = "";
			if(Origins != null)
			{
				foreach (SourceLocation origin in Origins)
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

		public List<string> GetAllRegionsAndCitiesList()
		{
			List<string> origins = new();
			if (Origins != null)
			{
				foreach (SourceLocation origin in Origins)
				{
					if (!String.IsNullOrEmpty(origin.City))
					{
						origins.Add(origin.City);
					}

					if (!String.IsNullOrEmpty(origin.Region))
					{
						origins.Add(origin.Region);
					}
				}
			}

			return origins;
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
					if (origin.Country != SourceCountry.Unknown)
					{
						countries.Add(origin.Country);
					}
				}
			}

			return countries;
		}

		#endregion
	}
}