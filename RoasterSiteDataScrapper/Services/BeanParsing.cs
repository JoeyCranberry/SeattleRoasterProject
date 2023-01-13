using RoasterBeansDataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RoasterBeansDataAccess.Models.BeanOrigin;

namespace RoasterBeansDataAccess.Services
{
	public class BeanNameParsing
	{
		public static List<SourceCountry> GetCountriesFromName(string beanName)
		{
			List<SourceCountry> countriesFromName = new List<SourceCountry>();

			foreach (var country in Enum.GetValues<SourceCountry>())
			{
				if (beanName.ToLower().Contains(country.ToString().ToLower().Replace("_", " ")))
				{
					countriesFromName.Add(country);
				}
			}

			if (beanName.ToLower().Contains("sumatra") && !countriesFromName.Contains(SourceCountry.INDONESIA))
			{
				countriesFromName.Add(SourceCountry.INDONESIA);
			}

			if (beanName.ToLower().Contains("congo") && !countriesFromName.Contains(SourceCountry.DEMOCRATIC_REPUBLIC_OF_THE_CONGO))
			{
				countriesFromName.Add(SourceCountry.DEMOCRATIC_REPUBLIC_OF_THE_CONGO);
			}

			return countriesFromName;
		}

		public static List<ProcessingMethod> GetProcessFromName(string beanName)
		{
			var results = new List<ProcessingMethod>();

			foreach (var process in Enum.GetValues<ProcessingMethod>())
			{
				if (beanName.ToLower().Contains(process.ToString().ToLower().Replace("_", " ")))
				{
					results.Add(process);
				}
			}

			return results;
		}

		public static bool GetIsDecafFromName(string beanName)
		{
			if (beanName.ToLower().Contains(value: "decaf"))
			{
				return true;
			}

			return false;
		}

		public static OrganicCerification GetOrganicFromName(string beanName)
		{
			if (beanName.ToLower().Contains(value: "organic") || beanName.ToLower().Contains(value: "fto"))
			{
				if (beanName.ToLower().Contains(value: "usda"))
				{
					return OrganicCerification.CERTIFIED_ORGANIC;
				}
				else
				{
					return OrganicCerification.UNCERTIFIED_ORGANIC;
				}
			}

			return OrganicCerification.NOT_ORGANIC;
		}

		public static RoastLevel SetRoastLevelFromName(string beanName)
		{
			foreach (var roast in Enum.GetValues<RoastLevel>())
			{
				if (beanName.ToLower().Contains(roast.ToString().ToLower().Replace("_", " ")))
				{
					return roast;
				}
			}

			return RoastLevel.UNKNOWN;
		}

		public static bool SetIsFairTradeFromName(string beanName)
		{
			if (beanName.ToLower().Contains(value: "fair-trade") || beanName.ToLower().Contains(value: "fair trade") || beanName.ToLower().Contains(value: "fto"))
			{
				return true;
			}

			return false;
		}
	}
}
