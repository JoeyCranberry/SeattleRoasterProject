﻿using RoasterBeansDataAccess.DataAccess;
using RoasterBeansDataAccess.Models;

namespace SeattleRoasterProject.Data.Services
{
	public class BeanFilterService
	{
		/*
		* Takes in the FilterModel and builds a BeanFilter from its elements
		*
		* SearchTerms extracts terms like roast level, organic certification, etc. by searching the string and
		* determing whether a filter should be active and what the compare value(s) should be
		*
		* E.g. a search like "Ethiopian single-origin organic"
		* builds a filter to only pull beans with Ethiopia in the CountriesOfOrigin, IsSingleOrigin = true, and OrganicCerification == CERTIFIED_ORGANIC or UNCERTIFIED_ORGANIC
		*/
		public BeanFilter BuildFilterFromSearchTerms(string searchTerms)
		{
			string cleanedSearchTerms = searchTerms.ToLower();

			BeanFilter newFilter = new BeanFilter()
			{
				IsExcluded = new FilterValueBool(true, false)
			};

			// Check if the search name contains roast level terms
			var roastFilterFromSearch = GetRoastLevelFilter(cleanedSearchTerms);
			cleanedSearchTerms = roastFilterFromSearch.newSearchTerms;
			newFilter.RoastFilter = roastFilterFromSearch.roastFilter;

			// Check for Country names
			var countryFilterFromSearch = GetCountryFilter(cleanedSearchTerms);
			cleanedSearchTerms = countryFilterFromSearch.newSearchTerms;
			newFilter.CountryFilter = countryFilterFromSearch.countryFilter;

			// Check for processes
			var processFilterFromSearch = GetProcessingFilter(cleanedSearchTerms);
			cleanedSearchTerms = processFilterFromSearch.newSearchTerms;
			newFilter.ProcessFilter = processFilterFromSearch.processFilter;

			// Check for organic
			var organicFilterFromSearch = GetOrganicFilter(cleanedSearchTerms);
			cleanedSearchTerms = organicFilterFromSearch.newSearchTerms;
			newFilter.OrganicFilter = organicFilterFromSearch.organicFilter;

			// Check for single origin or blend
			var originsFilterFromSearch = GetSingleOriginAndBlendFilter(cleanedSearchTerms);
			cleanedSearchTerms = originsFilterFromSearch.newSearchTerms;
			newFilter.IsSingleOrigin = originsFilterFromSearch.originsFilter;

			// Check for fair trade
			var fairTradeFilterFromSearch = GetFairTradeFilter(cleanedSearchTerms);
			cleanedSearchTerms = fairTradeFilterFromSearch.newSearchTerms;
			newFilter.IsFairTradeCertified = fairTradeFilterFromSearch.fairTradeFilter;

			// Check for direct trade
			var directTradeFilterFromSearch = GetDirectTradeFilter(cleanedSearchTerms);
			cleanedSearchTerms = directTradeFilterFromSearch.newSearchTerms;
			newFilter.IsDirectTradeCertified = directTradeFilterFromSearch.directTradeFilter;

			// Check for decaf or caffeinated
			var caffeineFilterFromSearch = GetCaffeineFilter(cleanedSearchTerms);
			cleanedSearchTerms = caffeineFilterFromSearch.newSearchTerms;
			newFilter.IsDecaf = caffeineFilterFromSearch.caffeineFilter;

			cleanedSearchTerms = cleanedSearchTerms.Trim();

			newFilter.SearchNameString = new FilterSearchString(!String.IsNullOrEmpty(cleanedSearchTerms), cleanedSearchTerms);

			return newFilter;
		}

		#region Filter Builders

		private (FilterList<RoastLevel> roastFilter, string newSearchTerms) GetRoastLevelFilter(string searchTerms)
		{
			FilterList<RoastLevel> roastFilter = new FilterList<RoastLevel>(false, new List<RoastLevel>());

			if (searchTerms.Contains("light"))
			{
				roastFilter = new FilterList<RoastLevel>(true, new List<RoastLevel>() { RoastLevel.LIGHT });
				searchTerms = searchTerms.Replace("light", "").Trim();
			}
			else if (searchTerms.Contains("medium"))
			{
				roastFilter = new FilterList<RoastLevel>(true, new List<RoastLevel>() { RoastLevel.MEDIUM });
				searchTerms = searchTerms.Replace("medium", "").Trim();
			}
			else if (searchTerms.Contains("dark"))
			{
				roastFilter = new FilterList<RoastLevel>(true, new List<RoastLevel>() { RoastLevel.DARK });
				searchTerms = searchTerms.Replace("medium", "").Trim();
			}

			return (roastFilter, searchTerms);
		}

		private (FilterList<Country> countryFilter, string newSearchTerms) GetCountryFilter(string searchTerms)
		{
			FilterList<Country> countryFilter = new FilterList<Country>(false, new List<Country>());
			List<Country> countriesInSearch = new List<Country>();

			foreach (Country country in Enum.GetValues<Country>())
			{
				string countrySearchTerm = country.ToString().Replace("_", " ").ToLower();
				if (searchTerms.Contains(countrySearchTerm))
				{
					countriesInSearch.Add(country);
					// Handles both Ethiopia and Ethiopian or El Salvador and El Salvadorian
					searchTerms = searchTerms
						.Replace(BeanModel.GetCountryPossesiveTerm(country).ToLower(), "")
						.Replace(countrySearchTerm, "")
						.Trim();
				}
			}

			if (countriesInSearch.Count > 0)
			{
				countryFilter = new FilterList<Country>(true, countriesInSearch);
			}

			return (countryFilter, searchTerms);
		}

		private (FilterList<BeanProcessing> processFilter, string newSearchTerms) GetProcessingFilter(string searchTerms)
		{
			FilterList<BeanProcessing> processFilter = new FilterList<BeanProcessing>(false, new List<BeanProcessing>());
			List<BeanProcessing> processesInSearch = new List<BeanProcessing>();

			foreach (BeanProcessing process in Enum.GetValues<BeanProcessing>())
			{
				string processSearchTerm = process.ToString().Replace("_", " ").ToLower();
				if (searchTerms.Contains(processSearchTerm))
				{
					processesInSearch.Add(process);

					searchTerms = searchTerms
						.Replace(processSearchTerm, "")
						.Trim();
				}
			}

			if (processesInSearch.Count > 0)
			{
				processFilter = new FilterList<BeanProcessing>(true, processesInSearch);
			}

			return (processFilter, searchTerms);
		}

		private (FilterList<OrganicCerification> organicFilter, string newSearchTerms) GetOrganicFilter(string searchTerms)
		{
			FilterList<OrganicCerification> organicFilter = new FilterList<OrganicCerification>(false, new List<OrganicCerification>());

			// Has organic, check if organic, and then if search included certidication terms
			if (searchTerms.Contains("organic"))
			{
				searchTerms = searchTerms.Replace("organic", "");

				if ((searchTerms.Contains("usda") || (searchTerms.Contains("certified") && !searchTerms.Contains("uncertified"))))
				{
					searchTerms = searchTerms.Replace("usda", "")
						.Replace("certified", "");
					organicFilter = new FilterList<OrganicCerification>(true, new List<OrganicCerification>() { OrganicCerification.CERTIFIED_ORGANIC });
				}
				else if (searchTerms.Contains("uncertified"))
				{
					searchTerms = searchTerms.Replace("uncertified", "");
					organicFilter = new FilterList<OrganicCerification>(true, new List<OrganicCerification>() { OrganicCerification.UNCERTIFIED_ORGANIC });
				}
				else
				{
					organicFilter = new FilterList<OrganicCerification>(true, new List<OrganicCerification>() { OrganicCerification.CERTIFIED_ORGANIC, OrganicCerification.UNCERTIFIED_ORGANIC });
				}
			}

			return (organicFilter, searchTerms);

		}

		private (FilterValueBool originsFilter, string newSearchTerms) GetSingleOriginAndBlendFilter(string searchTerms)
		{
			FilterValueBool originsFilter = new FilterValueBool(false, false);

			if (searchTerms.Contains("single origin") || searchTerms.Contains("single-origin"))
			{
				searchTerms = searchTerms
					.Replace("single origin", "")
					.Replace("single-origin", "");
				originsFilter = new FilterValueBool(true, true);
			}
			else if (searchTerms.Contains("blend"))
			{
				searchTerms = searchTerms.Replace("blend", "");
				originsFilter = new FilterValueBool(true, false);
			}

			return (originsFilter, searchTerms);
		}

		private (FilterValueBool fairTradeFilter, string newSearchTerms) GetFairTradeFilter(string searchTerms)
		{
			FilterValueBool fairTradeFilter = new FilterValueBool(false, false);

			if (searchTerms.Contains("fair trade"))
			{
				searchTerms = searchTerms.Replace("fair trade", "");
				fairTradeFilter = new FilterValueBool(true, true);
			}

			return (fairTradeFilter, searchTerms);
		}

		private (FilterValueBool directTradeFilter, string newSearchTerms) GetDirectTradeFilter(string searchTerms)
		{
			FilterValueBool directTradeFilter = new FilterValueBool(false, false);

			if (searchTerms.Contains("direct trade"))
			{
				searchTerms = searchTerms.Replace("direct trade", "");
				directTradeFilter = new FilterValueBool(true, true);
			}

			return (directTradeFilter, searchTerms);
		}

		private (FilterValueBool caffeineFilter, string newSearchTerms) GetCaffeineFilter(string searchTerms)
		{
			FilterValueBool isDecafFilter = new FilterValueBool(false, false);

			List<string> caffeinatedTerms = new List<string>() { "not decaf", "non-decaf", "caffeine", "caffeinated" };
			List<string> decafTerms = new List<string>() { "decaf", "not caffeinated", "no caffeine" };
			List<string> splitSearchTerms = searchTerms.Split(" ").ToList();

			if (splitSearchTerms.Intersect(caffeinatedTerms).Any())
			{
				splitSearchTerms.RemoveAll(t => caffeinatedTerms.Contains(t));

				isDecafFilter = new FilterValueBool(true, false);
			}
			else if (splitSearchTerms.Intersect(decafTerms).Any())
			{
				splitSearchTerms.RemoveAll(t => decafTerms.Contains(t));

				isDecafFilter = new FilterValueBool(true, true);
			}

			return (isDecafFilter, searchTerms);
		}

		#endregion
	}
}