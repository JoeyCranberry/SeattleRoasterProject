using RoasterBeansDataAccess.Models;
using SeattleRoasterProject.Pages;
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
						SearchSuggestion.SuggestionType.PROCESSING_METHOD
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
						new List<string>() { BeanOrigin.GetCountryDemonym(country) }
					)
				);
			}

			// Add roast levels
			foreach (var level in Enum.GetValues<RoastLevel>().Where(m => m != RoastLevel.UNKNOWN))
			{
				suggestions.Add(
					 new SearchSuggestion(
						BeanModel.GetTitleCase(level.ToString().Replace("_", " ") + " roast"),
						SearchSuggestion.SuggestionType.ROAST_LEVEL
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
									suggestions.Add(new SearchSuggestion(fullRegion, SearchSuggestion.SuggestionType.REGION));
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
						suggestions.Add(new SearchSuggestion(roaster.Name, SearchSuggestion.SuggestionType.REGION));
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

		public List<SearchSuggestion> GetSuggestionMatches(List<SearchSuggestion> allSuggestions, string search, int maxSuggestions = 10)
		{
			List<SearchSuggestion> matchingSuggestions = new();

			List<string> inputWordSplit = search.Split(' ').ToList();
			string lastInputWord = inputWordSplit.Last() ?? "";

			// Match based on starts with
			foreach (SearchSuggestion suggestion in allSuggestions)
			{
				foreach (string term in suggestion.MatchingStrings)
				{
					if (term.ToLower().StartsWith(lastInputWord.ToLower()) && matchingSuggestions.Count < maxSuggestions)
					{
						matchingSuggestions.Add(suggestion);
						break;
					}
				}
			}
			
			// If there are no exact matches try and match by contains instead
			if (matchingSuggestions.Count == 0)
			{
				foreach (SearchSuggestion suggestion in allSuggestions)
				{
					foreach (string term in suggestion.MatchingStrings)
					{
						if (term.ToLower().Contains(lastInputWord.ToLower()))
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
		public List<string> MatchingStrings { get; set; } = new();
		public string OptionClass { get; set; } = string.Empty;
		public SuggestionType SuggestionCategory { get; set; } = SuggestionType.OTHER;

		public SearchSuggestion(string _displayName, SuggestionType _category)
		{
			DisplayName = _displayName;
			MatchingStrings = new() { _displayName };
			OptionClass = "";
			SuggestionCategory = _category;
		}

		public SearchSuggestion(string _displayName, SuggestionType _category, List<string> additionalMatchingStrings)
		{
			DisplayName = _displayName;
			MatchingStrings = new() { _displayName };
			MatchingStrings.AddRange(additionalMatchingStrings);
			OptionClass = "";
			SuggestionCategory = _category;
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
		public string SearchTextRemoved { get; set; } = String.Empty;

		public SearchSuggestionAction(SearchSuggestion? suggestion, ActionType type, string textRemoved)
		{
			Suggestion = suggestion;
			Type = type;
			SearchTextRemoved = textRemoved;
		}

		public enum ActionType
		{
			Added,
			Removed
		}
}
}
