using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoasterBeansDataAccess.Models
{
	public class BeanOrigin
	{
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
					return "Guatemalan";
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

		public static string GetOriginLongDisplay(SourceLocation origin, bool includeFlag = false)
		{
			string originLongName = "";

			if (includeFlag)
			{
				if (origin.Country != SourceCountry.UNKNOWN)
				{
					originLongName += GetCountryFlag(origin.Country) + " ";
				}
				else if (origin.Continent.HasValue)
				{
					return GetContinentDisplayName(origin.Continent.Value);
				}
			}


			if (!String.IsNullOrEmpty(origin.Region))
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
		private static string GetTitleCase(string input)
		{
			TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
			return textInfo.ToTitleCase(input.ToLower().Replace("_", " "));
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
	}
}
