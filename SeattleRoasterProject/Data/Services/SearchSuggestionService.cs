using RoasterBeansDataAccess.DataAccess;
using RoasterBeansDataAccess.Models;
using SeattleRoasterProject.Pages;
using System.Diagnostics.Metrics;
using System.Reflection.Emit;
using static RoasterBeansDataAccess.Models.BeanOrigin;

namespace SeattleRoasterProject.Data.Services
{
	public class SearchSuggestionService
	{
		public async Task<List<SearchSuggestion>> BuildSuggestions(List<RoasterModel> roasters, List<BeanModel> beans, List<TastingNoteModel> allTastingNotes)
		{
			List<SearchSuggestion> suggestions = new();

			// Add processing methods
			foreach (var method in Enum.GetValues<ProcessingMethod>().Where(m => m != ProcessingMethod.UNKNOWN))
			{
				if (beans != null && beans.Where(b => b.ProcessingMethods != null && b.ProcessingMethods.Contains(method)).Any())
					suggestions.Add(
					 new SearchSuggestion(
						BeanModel.GetTitleCase(method.ToString().Replace("_", " ")),
						SearchSuggestion.SuggestionType.PROCESSING_METHOD,
						(int)method
					)
				);
			}

			// Add countries and demonyms
			foreach (var country in Enum.GetValues<SourceCountry>().Where(c => c != SourceCountry.UNKNOWN))
			{
				if(beans != null && beans.Where(b => b.GetOriginCountries().Contains(country)).Any())
				{
					suggestions.Add(
					 new SearchSuggestion(
						BeanOrigin.GetCountryDisplayName(country),
						SearchSuggestion.SuggestionType.COUNTRY,
						new List<string>() { BeanOrigin.GetCountryDemonym(country) },
						(int)country,
						null
					)
					);
				}
			}

			// Add roast levels
			foreach (var level in Enum.GetValues<RoastLevel>().Where(m => m != RoastLevel.UNKNOWN))
			{
				suggestions.Add(
					 new SearchSuggestion(
						BeanModel.GetTitleCase(level.ToString().Replace("_", " ") + " roast"),
						SearchSuggestion.SuggestionType.ROAST_LEVEL,
						(int)level
					)
				);
			}

			// Add bean regions
			if (beans != null)
			{
				foreach (BeanModel bean in beans)
				{
					if (bean.Origins != null)
					{
						foreach (SourceLocation origin in bean.Origins)
						{
							if (!String.IsNullOrEmpty(origin.Region))
							{
								string fullRegion = origin.Region + ", " + BeanOrigin.GetCountryDisplayName(origin.Country);
								if (!suggestions.Where(s => s.DisplayName == fullRegion).Any())
								{
									suggestions.Add(new SearchSuggestion(
										fullRegion,
										SearchSuggestion.SuggestionType.REGION,
										(int)origin.Country // Regions aren't enums, but we can capture the country
									));
								}
							}
						}
					}
				}
			}

			// Add roaster names
			if (roasters != null)
			{
				foreach (RoasterModel roaster in roasters)
				{
					if (!String.IsNullOrEmpty(roaster.Name))
					{
						if(beans != null && beans.Where(b => b.MongoRoasterId == roaster.Id).Any())
						{
							suggestions.Add(new SearchSuggestion(
								roaster.Name,
								SearchSuggestion.SuggestionType.ROASTER,
								roaster.Id
							));
						}
						
					}
				}
			}

			suggestions.AddRange(GetSpecialSuggestions());

			suggestions.AddRange(GetTastingNoteSuggestions(allTastingNotes));

			return suggestions;
		}

		public List<SearchSuggestion> GetSuggestionMatches(List<SearchSuggestion> allSuggestions, string search, List<SearchSuggestion> acceptedSuggestions, int maxSuggestions = 10)
		{
			List<SearchSuggestion> matchingSuggestions = new();

			List<string> inputWordSplit = search.Split(' ').ToList();

			// Pupled Natural San Pedro, Guatemala
			// 0	  1		  2   3		 4
			for(int i = inputWordSplit.Count - 1; i >= 0; i--)
			{
				string largestSearchTerm = "";
				for(int j = i; j < inputWordSplit.Count; j++)
				{
					largestSearchTerm += inputWordSplit[j] + " ";
				}

				largestSearchTerm = largestSearchTerm.Trim();

				List<SearchSuggestion> exactMatches = GetSuggestionExactMatches(allSuggestions, largestSearchTerm, acceptedSuggestions, maxSuggestions);
				
				// If some number of exact matches were returned, then override the exact matches set by a small combination of terms
				if(exactMatches.Count > 0)
				{
					matchingSuggestions = exactMatches;
				}
			}

			// If there are no exact matches try and match by contains instead
			if (matchingSuggestions.Count == 0)
			{
				for (int i = inputWordSplit.Count; i > 0; i--)
				{
					string largestSearchTerm = "";
					for (int j = i; j < inputWordSplit.Count; j++)
					{
						largestSearchTerm += inputWordSplit[j] + "";
					}

					List<SearchSuggestion> exactMatches = GetSuggestionApproximateMatches(allSuggestions, largestSearchTerm, acceptedSuggestions, maxSuggestions);
					
					if (exactMatches.Count > 0)
					{
						matchingSuggestions = exactMatches;
					}
				}
			}

			return matchingSuggestions.Take(maxSuggestions).ToList();
		}

		public BeanFilter GetFilterFromSuggestions(List<SearchSuggestion> acceptedSuggestions)
		{
			BeanFilter filter = new BeanFilter();

			foreach (SearchSuggestion suggestion in acceptedSuggestions) 
			{ 
				switch(suggestion.SuggestionCategory)
				{
					case SearchSuggestion.SuggestionType.ROASTER:
						// Add roaster id to chosen Roasters list
						if(!String.IsNullOrEmpty(suggestion.MatchingIdValue))
						{
							filter.ChosenRoasters.IsActive = true;
							filter.ChosenRoasters.CompareValues.Add(suggestion.MatchingIdValue);
						}
						break;
					case SearchSuggestion.SuggestionType.REGION:
						if (suggestion.MatchingIntValue != null)
						{
							filter.CountryFilter.IsActive = true;
							filter.CountryFilter.CompareValues.Add((SourceCountry)suggestion.MatchingIntValue);

							filter.RegionFilter.IsActive = true;
							int commaIndex = suggestion.DisplayName.IndexOf(',');
							if(commaIndex != -1)
							{
								string region = suggestion.DisplayName.Substring(0, commaIndex);
								filter.RegionFilter.CompareValues.Add(region);
							}
						}
						break;
					case SearchSuggestion.SuggestionType.COUNTRY:
						if (suggestion.MatchingIntValue != null)
						{
							filter.CountryFilter.IsActive = true;
							filter.CountryFilter.CompareValues.Add((SourceCountry)suggestion.MatchingIntValue);
						}
						break;
					case SearchSuggestion.SuggestionType.ROAST_LEVEL:
						if (suggestion.MatchingIntValue != null)
						{
							filter.RoastFilter.IsActive = true;
							filter.RoastFilter.CompareValues.Add((RoastLevel)suggestion.MatchingIntValue);
						}
						break;
					case SearchSuggestion.SuggestionType.PROCESSING_METHOD:
						if (suggestion.MatchingIntValue != null)
						{
							filter.ProcessFilter.IsActive = true;
							filter.ProcessFilter.CompareValues.Add((ProcessingMethod)suggestion.MatchingIntValue);
						}
						break;
					case SearchSuggestion.SuggestionType.SPECIAL:
						switch (suggestion.MatchingIntValue)
						{
							case 0:
								filter.AvailablePreground = new FilterValueBool(true, true);
								break;
							case 1:
								filter.IsSingleOrigin = new FilterValueBool(true, true);
								break;
							case 2:
								filter.IsSingleOrigin = new FilterValueBool(true, false);
								break;
							case 3:
								filter.IsDecaf = new FilterValueBool(true, true);
								break;
							case 4:
								filter.IsFairTradeCertified = new FilterValueBool(true, true);
								break;
							case 5:
								filter.IsDirectTradeCertified = new FilterValueBool(true, true);
								break;
							case 6:
								filter.IsFromWomanOwnedFarms = new FilterValueBool(true, true);
								break;
							case 7:
								filter.IsSupportingCause = new FilterValueBool(true, true);
								break;
						}
						break;
					case SearchSuggestion.SuggestionType.TASTING_NOTE:
						if(suggestion.MatchingStrings != null)
						{
							filter.TastingNotesFilter.IsActive = true;
							filter.TastingNotesFilter.CompareValues.AddRange(suggestion.MatchingStrings);
						}
						break;
					// By default add to the search name string
					default:
						filter.SearchNameString.IsActive = true;
						filter.SearchNameString.CompareString += suggestion.DisplayName + " ";
						break;
				}
			}

			return filter;
		}

		private List<SearchSuggestion> GetSuggestionExactMatches(List<SearchSuggestion> allSuggestions, string search, List<SearchSuggestion> acceptedSuggestions, int maxSuggestions = 10)
		{
			List<SearchSuggestion> matchingSuggestions = new();

			foreach (SearchSuggestion suggestion in allSuggestions)
			{
				// If suggestion isn't already accepted
				if (!acceptedSuggestions.Where(vs => vs.DisplayName == suggestion.DisplayName && vs.SuggestionCategory == suggestion.SuggestionCategory).Any())
				{
					foreach (string term in suggestion.MatchingStrings)
					{
						if (term.ToLower().StartsWith(search.ToLower()) && matchingSuggestions.Count < maxSuggestions)
						{
							matchingSuggestions.Add(suggestion);
							break;
						}
					}
				}
			}

			return matchingSuggestions;
		}

		private List<SearchSuggestion> GetSuggestionApproximateMatches(List<SearchSuggestion> allSuggestions, string search, List<SearchSuggestion> acceptedSuggestions, int maxSuggestions = 10)
		{
			List<SearchSuggestion> matchingSuggestions = new();

			foreach (SearchSuggestion suggestion in allSuggestions)
			{
				if (!acceptedSuggestions.Contains(suggestion))
				{
					foreach (string term in suggestion.MatchingStrings)
					{
						if (term.ToLower().Contains(search.ToLower()))
						{
							matchingSuggestions.Add(suggestion);
							break;
						}
					}
				}
			}

			return matchingSuggestions;
		}
	
		private List<SearchSuggestion> GetSpecialSuggestions()
		{
			List<SearchSuggestion> specialSuggestions = new()
			{
				new SearchSuggestion("Pre-Ground", SearchSuggestion.SuggestionType.SPECIAL, 0),
				new SearchSuggestion("Single-Origin", SearchSuggestion.SuggestionType.SPECIAL, 1),
				new SearchSuggestion("Blend", SearchSuggestion.SuggestionType.SPECIAL, 2),
				new SearchSuggestion("Decaf", SearchSuggestion.SuggestionType.SPECIAL, 3),
				new SearchSuggestion("Fair Trade", SearchSuggestion.SuggestionType.SPECIAL, 4),
				new SearchSuggestion("Direct Trade", SearchSuggestion.SuggestionType.SPECIAL, 5),
				new SearchSuggestion("Woman-Owned", SearchSuggestion.SuggestionType.SPECIAL, 6),
				new SearchSuggestion("Supports Cause", SearchSuggestion.SuggestionType.SPECIAL, 7)
			};

			return specialSuggestions;
		}

		private List<SearchSuggestion> GetTastingNoteSuggestions(List<TastingNoteModel> allTastingNotes)
		{
			var searchSuggestions = allTastingNotes.Select(note => new SearchSuggestion(note.NoteName, SearchSuggestion.SuggestionType.TASTING_NOTE, note.Aliases ?? new List<string>(), null, null));

			return searchSuggestions.ToList();
		}
	}

		public class SearchSuggestion
		{
		public string DisplayName { get; set; } = String.Empty;
		public List<string>? MatchingStrings { get; set; }
		public string OptionClass { get; set; } = string.Empty;
		public SuggestionType SuggestionCategory { get; set; } = SuggestionType.OTHER;
		public int? MatchingIntValue { get; set; }
		public string? MatchingIdValue { get; set; }

		public SearchSuggestion(string _displayName, SuggestionType _category, int? _matchingIntValue) : this(_displayName, _category, new List<string>() { _displayName }, _matchingIntValue, null)
		{

		}

		public SearchSuggestion(string _displayName, SuggestionType _category) : this(_displayName, _category, new List<string>() { _displayName }, null, null)
		{

		}

		public SearchSuggestion(string _displayName, SuggestionType _category, string? _matchingIdValue) : this(_displayName, _category, new List<string>() { _displayName }, null, _matchingIdValue)
		{

		}

		public SearchSuggestion(string _displayName, SuggestionType _category, List<string> additionalMatchingStrings, int? _matchingIntValue, string? _matchingIdValue)
		{
			DisplayName = _displayName;
			MatchingStrings = new() { _displayName };
			MatchingStrings.AddRange(additionalMatchingStrings);
			SuggestionCategory = _category;
			MatchingIntValue = _matchingIntValue;
			MatchingIdValue = _matchingIdValue;
		}

		public enum SuggestionType
		{ 
			ROASTER,
			REGION,
			COUNTRY,
			ROAST_LEVEL,
			PROCESSING_METHOD,
			SPECIAL,
			OTHER,
			TASTING_NOTE
		}
	}

	public class SearchSuggestionAction
	{
		public SearchSuggestion? Suggestion { get; set; }
		public ActionType Type { get; set; }

		public SearchSuggestionAction(SearchSuggestion? suggestion, ActionType type)
		{
			Suggestion = suggestion;
			Type = type;
		}

		public enum ActionType
		{
			Added,
			Removed
		}
	}
}
