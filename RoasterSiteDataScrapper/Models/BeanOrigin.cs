using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SeattleRoasterProject.Core.Enums;

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
				case SourceCountry.Democratic_Republic_Of_The_Congo:
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
				case SourceCountry.Ethiopia:
					return "🇪🇹";
				case SourceCountry.Colombia:
					return "🇨🇴";
				case SourceCountry.Rwanda:
					return "🇷🇼";
				case SourceCountry.Guatemala:
					return "🇬🇹";
				case SourceCountry.El_Salvador:
					return "🇸🇻";
				case SourceCountry.Indonesia:
					return "🇮🇩";
				case SourceCountry.Honduras:
					return "🇭🇳";
				case SourceCountry.Nicaragua:
					return "🇳🇮";
				case SourceCountry.Brazil:
					return "🇧🇷";
				case SourceCountry.Kenya:
					return "🇰🇪";
				case SourceCountry.Mexico:
					return "🇲🇽";
				case SourceCountry.Costa_Rica:
					return "🇨🇷";
				case SourceCountry.Papua_New_Guinea:
					return "🇵🇬";
				case SourceCountry.Peru:
					return "🇵🇪";
				case SourceCountry.Uganda:
					return "🇺🇬";
				case SourceCountry.Burundi:
					return "🇧🇮";
				case SourceCountry.Democratic_Republic_Of_The_Congo:
					return "🇨🇩";
				case SourceCountry.Tanzania:
					return "🇹🇿";
				case SourceCountry.East_Timor:
					return "🇹🇱";
				case SourceCountry.Dominican_Republic:
					return "🇩🇴";
				case SourceCountry.Vietnam:
					return "🇻🇳";
				case SourceCountry.Ecuador:
					return "🇪🇨";
				case SourceCountry.China:
					return "🇨🇳";
				case SourceCountry.Myanmar:
					return "🇲🇲";
				case SourceCountry.Thailand:
					return "🇹🇭";
				case SourceCountry.Haiti:
					return "🇭🇹";
				case SourceCountry.Yemen:
					return "🇾🇪";
				case SourceCountry.Bolivia:
					return "🇧🇴";
				case SourceCountry.Philippines:
					return "🇵🇭";
				default:
					return "🌎";
			}
		}

		public static string GetContinentDisplayName(SourceContinent region)
		{
			string worldString = "";
			switch (region)
			{
				case SourceContinent.South_America:
				case SourceContinent.Central_America:
					worldString = "🌎 ";
					break;
				case SourceContinent.Africa:
					worldString = "🌍 ";
					break;
				case SourceContinent.Asia:
					worldString = "🌏 ";
					break;
			}

			return worldString + GetTitleCase(region.ToString());
		}

		public static string GetCountryDemonym(SourceCountry country)
		{
			switch (country)
			{
				case SourceCountry.Ethiopia:
					return "Ethiopian";
				case SourceCountry.Colombia:
					return "Colombian";
				case SourceCountry.Rwanda:
					return "Rwandan";
				case SourceCountry.Guatemala:
					return "Guatemalan";
				case SourceCountry.El_Salvador:
					return "El Salvadorian";
				case SourceCountry.Indonesia:
					return "Indonesian";
				case SourceCountry.Honduras:
					return "Honduran";
				case SourceCountry.Nicaragua:
					return "Nicaraguan";
				case SourceCountry.Brazil:
					return "Brazilian";
				case SourceCountry.Kenya:
					return "Kenyan";
				case SourceCountry.Mexico:
					return "Mexican";
				case SourceCountry.Costa_Rica:
					return "Costa Rican";
				case SourceCountry.Papua_New_Guinea:
					return "Papua New Guinean";
				case SourceCountry.Peru:
					return "Peruvian";
				case SourceCountry.Uganda:
					return "Ugandan";
				case SourceCountry.Burundi:
					return "Umurundi";
				case SourceCountry.Democratic_Republic_Of_The_Congo:
					return "Congolese";
				case SourceCountry.Tanzania:
					return "Tanzanian";
				case SourceCountry.East_Timor:
					return "Timorese";
				case SourceCountry.Dominican_Republic:
					return "Dominican";
				case SourceCountry.Vietnam:
					return "Vietnamese";
				case SourceCountry.Ecuador:
					return "Ecuadorian";
				case SourceCountry.China:
					return "Chinese";
				case SourceCountry.Myanmar:
					return "Burmese";
				case SourceCountry.Thailand:
					return "Thai";
				case SourceCountry.Haiti:
					return "Haitian";
				case SourceCountry.Yemen:
					return "Yemeni";
				case SourceCountry.Bolivia:
					return "Bolivian";
				case SourceCountry.Philippines:
					return "Philippines";
				default:
					return country.ToString();
			}
		}

		public static string GetOriginLongDisplay(SourceLocation origin, bool includeFlag = false)
		{
			string originLongName = "";

			if (includeFlag)
			{
				if (origin.Country != SourceCountry.Unknown)
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

			if (origin.Country != SourceCountry.Unknown)
			{
				originLongName += GetCountryDisplayName(origin.Country);
			}

			return originLongName;
		}
		private static string GetTitleCase(string input)
		{
			var textInfo = new CultureInfo("en-US", false).TextInfo;
			return textInfo.ToTitleCase(input.ToLower().Replace("_", " "));
		}
	}
}
