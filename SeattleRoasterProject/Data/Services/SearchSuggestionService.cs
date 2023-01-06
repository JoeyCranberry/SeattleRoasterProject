using RoasterBeansDataAccess.Models;
using SeattleRoasterProject.Pages;
using System.Diagnostics.Metrics;
using System.Reflection.Emit;
using static RoasterBeansDataAccess.Models.BeanOrigin;

namespace SeattleRoasterProject.Data.Services
{
	public class SearchSuggestionService
	{
		public List<SearchSuggestion> BuildSuggestions(List<RoasterModel> roasters, List<BeanModel> beans)
		{
			List<SearchSuggestion> suggestions = new();

			// Add processing methods
			foreach (var method in Enum.GetValues<ProccessingMethod>().Where(m => m != ProccessingMethod.UNKNOWN))
			{
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
						suggestions.Add(new SearchSuggestion(
								roaster.Name,
								SearchSuggestion.SuggestionType.REGION,
								roaster.Id
						));
					}
				}
			}

			suggestions.Add(new SearchSuggestion("Pre-Ground", SearchSuggestion.SuggestionType.SPECIAL));
			suggestions.Add(new SearchSuggestion("Single-Origin", SearchSuggestion.SuggestionType.SPECIAL));
			suggestions.Add(new SearchSuggestion("Decaf", SearchSuggestion.SuggestionType.SPECIAL));
			suggestions.Add(new SearchSuggestion("Fair Trade", SearchSuggestion.SuggestionType.SPECIAL));
			suggestions.Add(new SearchSuggestion("Direct Trade", SearchSuggestion.SuggestionType.SPECIAL));
			suggestions.Add(new SearchSuggestion("Woman-Owned", SearchSuggestion.SuggestionType.SPECIAL));
			suggestions.Add(new SearchSuggestion("Supports Cause", SearchSuggestion.SuggestionType.SPECIAL));


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
	}

		public class SearchSuggestion
		{
		public string DisplayName { get; set; } = String.Empty;
		public List<string>? MatchingStrings { get; set; }
		public string OptionClass { get; set; } = string.Empty;
		public SuggestionType SuggestionCategory { get; set; } = SuggestionType.OTHER;
		public int? MatchingEnumValue { get; set; }
		public string? MatchingIdValue { get; set; }

		public SearchSuggestion(string _displayName, SuggestionType _category, int? _matchingEnumValue) : this(_displayName, _category, new List<string>() { _displayName }, _matchingEnumValue, null)
		{

		}

		public SearchSuggestion(string _displayName, SuggestionType _category) : this(_displayName, _category, new List<string>() { _displayName }, null, null)
		{

		}

		public SearchSuggestion(string _displayName, SuggestionType _category, string? _matchingIdValue) : this(_displayName, _category, new List<string>() { _displayName }, null, _matchingIdValue)
		{

		}

		public SearchSuggestion(string _displayName, SuggestionType _category, List<string> additionalMatchingStrings, int? _matchingEnumValue, string? _matchingIdValue)
		{
			DisplayName = _displayName;
			MatchingStrings = new() { _displayName };
			MatchingStrings.AddRange(additionalMatchingStrings);
			SuggestionCategory = _category;
			MatchingEnumValue = _matchingEnumValue;
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
			OTHER
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
