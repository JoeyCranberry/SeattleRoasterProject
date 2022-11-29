using RoasterBeansDataAccess.DataAccess;
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
		public async Task<BeanFilter> BuildFilterFromSearchTerms(string searchTerms)
		{
			string cleanedSearchTerms = searchTerms.ToLower();

			BeanFilter newFilter = new BeanFilter()
			{
				IsExcluded = new FilterValueBool(true, false),
				IsInStock = new FilterValueBool(true, true)
			};

			if(searchTerms.Trim().Length == 0)
			{
				return newFilter;
			}

			cleanedSearchTerms = cleanedSearchTerms.Replace(",", "");

			// Check if the search name contains roast level terms
			var roastFilterFromSearch = GetRoastLevelFilter(cleanedSearchTerms);
			cleanedSearchTerms = roastFilterFromSearch.newSearchTerms.Trim();
			newFilter.RoastFilter = roastFilterFromSearch.roastFilter;

			// Check for Country names
			var countryFilterFromSearch = GetCountryFilter(cleanedSearchTerms);
			cleanedSearchTerms = countryFilterFromSearch.newSearchTerms.Trim();
			newFilter.CountryFilter = countryFilterFromSearch.countryFilter;

			// Check for processes
			var processFilterFromSearch = GetProcessingFilter(cleanedSearchTerms);
			cleanedSearchTerms = processFilterFromSearch.newSearchTerms.Trim();
			newFilter.ProcessFilter = processFilterFromSearch.processFilter;

			// Check for organic
			var organicFilterFromSearch = GetOrganicFilter(cleanedSearchTerms);
			cleanedSearchTerms = organicFilterFromSearch.newSearchTerms.Trim();
			newFilter.OrganicFilter = organicFilterFromSearch.organicFilter;

			// Check for pre-ground
			var pregroundFromSearch = GetAvailablePregroundFilter(cleanedSearchTerms);
			cleanedSearchTerms = pregroundFromSearch.newSearchTerms.Trim();
			newFilter.AvailablePreground = pregroundFromSearch.pregroundFilter;

			// Check for single origin or blend
			var originsFilterFromSearch = GetSingleOriginAndBlendFilter(cleanedSearchTerms);
			cleanedSearchTerms = originsFilterFromSearch.newSearchTerms.Trim();
			newFilter.IsSingleOrigin = originsFilterFromSearch.originsFilter;

			// Check for fair trade
			var fairTradeFilterFromSearch = GetFairTradeFilter(cleanedSearchTerms);
			cleanedSearchTerms = fairTradeFilterFromSearch.newSearchTerms.Trim();
			newFilter.IsFairTradeCertified = fairTradeFilterFromSearch.fairTradeFilter;

			// Check for direct trade
			var directTradeFilterFromSearch = GetDirectTradeFilter(cleanedSearchTerms);
			cleanedSearchTerms = directTradeFilterFromSearch.newSearchTerms.Trim();
			newFilter.IsDirectTradeCertified = directTradeFilterFromSearch.directTradeFilter;

			// Check for decaf or caffeinated
			var caffeineFilterFromSearch = GetCaffeineFilter(cleanedSearchTerms);
			cleanedSearchTerms = caffeineFilterFromSearch.newSearchTerms.Trim();
			newFilter.IsDecaf = caffeineFilterFromSearch.caffeineFilter;

			var supportsCauseFilterFromSearch = GetSupportsCauseFilter(cleanedSearchTerms);
			cleanedSearchTerms = supportsCauseFilterFromSearch.newSearchTerms.Trim();
			newFilter.IsSupportingCause = supportsCauseFilterFromSearch.isSupportingCauseFilter;

			var fromWomanOwnedFarmsFilterFromSearch = GetFromWomanOwnedFarmsFilter(cleanedSearchTerms);
			cleanedSearchTerms = fromWomanOwnedFarmsFilterFromSearch.newSearchTerms.Trim();
			newFilter.IsFromWomanOwnedFarms = fromWomanOwnedFarmsFilterFromSearch.isFromWomanOwnedFarms;

			var rainforestCertificationFilterFromSearch = GetRainforestAllianceCertified(cleanedSearchTerms);
			cleanedSearchTerms = rainforestCertificationFilterFromSearch.newSearchTerms.Trim();
			newFilter.IsRainforestAllianceCertified = rainforestCertificationFilterFromSearch.isRainforestAllianceCertified;

			var roasterNameSearch = await GetRoasterFilter(cleanedSearchTerms);
			cleanedSearchTerms = roasterNameSearch.newSearchTerms.Trim();
			newFilter.RoasterNameSearch = roasterNameSearch.roasterFilter;

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

			searchTerms = searchTerms.Replace("roast", "");

			return (roastFilter, searchTerms);
		}

		private (FilterList<Country> countryFilter, string newSearchTerms) GetCountryFilter(string searchTerms)
		{
			FilterList<Country> countryFilter = new FilterList<Country>(false, new List<Country>());
			List<Country> countriesInSearch = new List<Country>();

			foreach (Country country in Enum.GetValues<Country>())
			{
				string countrySearchTerm = country.ToString().Replace("_", " ").ToLower();
				string demonym = BeanModel.GetCountryDemonym(country).ToLower();
				if (searchTerms.Contains(countrySearchTerm) || searchTerms.Contains(demonym))
				{
					countriesInSearch.Add(country);
					// Handles both Ethiopia and Ethiopian or El Salvador and El Salvadorian
					searchTerms = searchTerms
						.Replace(demonym, "")
						.Replace(countrySearchTerm, "")
						.Trim();
				}
			}

			// Special cases
			if(searchTerms.Contains("congo "))
			{
				if(!countriesInSearch.Contains(Country.DEMOCRATIC_REPUBLIC_OF_THE_CONGO))
				{
					countriesInSearch.Add(Country.DEMOCRATIC_REPUBLIC_OF_THE_CONGO);
				}
			}

			if (countriesInSearch.Count > 0)
			{
				countryFilter = new FilterList<Country>(true, countriesInSearch);
			}

			return (countryFilter, searchTerms);
		}

		private (FilterList<ProccessingMethod> processFilter, string newSearchTerms) GetProcessingFilter(string searchTerms)
		{
			FilterList<ProccessingMethod> processFilter = new FilterList<ProccessingMethod>(false, new List<ProccessingMethod>());
			List<ProccessingMethod> processesInSearch = new List<ProccessingMethod>();

			foreach (ProccessingMethod process in Enum.GetValues<ProccessingMethod>())
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
				processFilter = new FilterList<ProccessingMethod>(true, processesInSearch);
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

		private (FilterValueBool pregroundFilter, string newSearchTerms) GetAvailablePregroundFilter(string searchTerms)
		{
			FilterValueBool pregroundFilter = new FilterValueBool(false, false);

			if (searchTerms.Contains("preground") || searchTerms.Contains("pre-ground") || searchTerms.Contains("ground"))
			{
				searchTerms = searchTerms
					.Replace("preground", "")
					.Replace("pre-ground", "")
					.Replace("ground", "");

				pregroundFilter = new FilterValueBool(true, true);
			}

			return (pregroundFilter, searchTerms);
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

		private async Task<(FilterList<string> roasterFilter, string newSearchTerms)> GetRoasterFilter(string searchTerms)
		{
			FilterList<string> roasterFilter = new FilterList<string>(false, new List<string>());

			// Remove common terms that are also part of roaster names, they will be added back later
			List<string> excludedTerms = new List<string>() { "espresso", "coffee" };
			List<string> removedTerms = new List<string>();

			foreach (string term in excludedTerms)
			{
				if (searchTerms.Contains(term))
				{
					searchTerms = searchTerms.Replace(term, newValue: "");
					removedTerms.Add(term);
				}
			}

			// Check for roaster names
			if (searchTerms.Length > 0)
			{
				RoasterService roasterServ = new RoasterService();
				var matchingRoasters = await roasterServ.GetRoastersByName(searchTerms);

				if(matchingRoasters != null && matchingRoasters.Count> 0)
				{
					List<string> roasterIds = new List<string>();
					foreach(RoasterModel roaster in matchingRoasters)
					{
						roasterIds.Add(roaster.Id);
						foreach (string roasterNamePart in roaster.Name.Split(' '))
						{
							searchTerms = searchTerms.Replace(roasterNamePart.ToLower(), "");
						}
					}

					return (new FilterList<string>(true, roasterIds), searchTerms);
				}
			}

			if (removedTerms.Count > 0)
			{
				searchTerms += " " + String.Join(" ", removedTerms);
			}

			return (roasterFilter, searchTerms);
		}

		private (FilterValueBool isSupportingCauseFilter, string newSearchTerms) GetSupportsCauseFilter(string searchTerms)
		{
			FilterValueBool isSupportingCauseFilter = new FilterValueBool(false, false);

			if (searchTerms.Contains("supports") && searchTerms.Contains("cause") )
			{
				searchTerms = searchTerms
					.Replace("supports", "")
					.Replace("support", "")
					.Replace("causes", "")
					.Replace("cause", "");

				isSupportingCauseFilter = new FilterValueBool(true, true);
			}

			return (isSupportingCauseFilter, searchTerms);
		}

		private (FilterValueBool isFromWomanOwnedFarms, string newSearchTerms) GetFromWomanOwnedFarmsFilter(string searchTerms)
		{
			FilterValueBool isFromWomanOwnedFarms = new FilterValueBool(false, false);

			if (searchTerms.Contains("woman") || searchTerms.Contains("women"))
			{
				searchTerms = searchTerms
					.Replace("woman", "")
					.Replace("women", "")
					.Replace("owned", "")
					.Replace("owned", "");

				isFromWomanOwnedFarms = new FilterValueBool(true, true);
			}

			return (isFromWomanOwnedFarms, searchTerms);
		}

		private (FilterValueBool isRainforestAllianceCertified, string newSearchTerms) GetRainforestAllianceCertified(string searchTerms)
		{
			FilterValueBool isRainforestAllianceCertified = new FilterValueBool(false, false);

			if (searchTerms.Contains("rainforest"))
			{
				searchTerms = searchTerms
					.Replace("rainforest", "")
					.Replace("alliance", "");

				isRainforestAllianceCertified = new FilterValueBool(true, true);
			}

			return (isRainforestAllianceCertified, searchTerms);
		}
		#endregion
	}
}
