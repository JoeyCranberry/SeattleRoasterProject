using RoasterBeansDataAccess.Models;
using static RoasterBeansDataAccess.Models.BeanOrigin;

namespace RoasterBeansDataAccess.Services
{
	/*
    * Get the score that collates all sourcing information and determines how traceable a bean is
    * For examples, beans that have no countries, but have continents will have a -5 score
    * 
    * But a bean that has countries, and a producer name will have a score of 5
    * 
    * Maximum score: 14
    * Minimum score: -5
    * 
    * Breakdown:
    * -5 -> -2: No sourcing, avoid
    * -3 -> 1: Bare mimimum
    * 2 -> 5: Good
    * 6 -> 8: Great
    * 9 -> 14: Amazing
    */
	public static class TraceabilityService
	{
		public static int GetTotalScore(BeanModel bean)
		{
			List<TraceabilityScore> scores = GetScoresForOrigins(bean);
			return scores.Sum(s => s.ScoreModifier);
		}

		public static string GetScoreBreakdownDisplay(BeanModel bean)
		{
			List<TraceabilityScore> scores = GetScoresForOrigins(bean);
			int totalScore = scores.Sum(s => s.ScoreModifier);

			string displayText = "<b>Traceability Score: " + totalScore + "</b>";
			foreach(TraceabilityScore score in scores)
			{
				displayText += TraceabilityScore.GetScoreNote(score.ScType);
			}

			return displayText;
		}

		public static string GetScoreStarDisplay(BeanModel bean)
		{
			int score = GetTotalScore(bean);

			int starHalves = 0;
			if (score <= -5)
			{
				starHalves = 0;
			}
			else if(score <= -2)
			{
				starHalves = 1;
			}
			else if(score <= 0)
			{
				starHalves = 2;
			}
			else if(score <= 2)
			{
				starHalves = 3;
			}
			else if(score <= 3)
			{
				starHalves = 4;
			}
			else if (score <= 5)
			{
				starHalves = 5;
			}
			else if(score <= 6)
			{
				starHalves = 6;
			}
			else if (score <= 8)
			{
				starHalves = 7;
			}
			else if (score <= 9)
			{
				starHalves = 8;
			}
			else if (score <= 11)
			{
				starHalves = 9;
			}
			else
			{
				starHalves = 10;
			}

			// Minimum score: -5
			// Maximum score: 15

			string result = "";
			for (int i = 0; i < 5; i++)
			{
				if (i < (starHalves / 2))
				{
					result += "<span class=\"material-symbols-outlined\">\r\nstar\r\n</span>";
				}
				else
				{
					if((starHalves / 2) == i && starHalves % 2 == 1)
					{
						result += "<span class=\"material-symbols-outlined\">\r\nstar_half\r\n</span>";
					}
					else
					{
						result += "<span class=\"material-symbols-outlined\">\r\nstar\r\n</span>";
					}
				}
			}

			return result;
		}

		private static List<TraceabilityScore> GetScoresForOrigins(BeanModel bean)
		{
			List<TraceabilityScore> scores = new();

			int regionsCount = 0;
			int countryCount = 0;
			int onlyContinentCount = 0;
			if (bean.Origins != null)
			{
				regionsCount = bean.Origins.Where(o => !String.IsNullOrEmpty(o.City) || !String.IsNullOrEmpty(o.Region)).Count();
				countryCount = bean.Origins.Where(o => o.Country != SourceCountry.UNKNOWN).Count();
				onlyContinentCount = bean.Origins.Where(o => o.Continent != null && o.Country == SourceCountry.UNKNOWN).Count();
			}

			// No sourcing information
			if (onlyContinentCount == 0 && countryCount == 0)
			{
				scores.Add(new TraceabilityScore(TraceabilityScore.ScoreType.NO_SOURCING));
			}
			else
			{
				// Only continents
				if (countryCount == 0)
				{
					scores.Add(new TraceabilityScore(TraceabilityScore.ScoreType.ONLY_CONTINENTS));
				}
				// Some countries
				else
				{
					if (onlyContinentCount > 0)
					{
						scores.Add(new TraceabilityScore(TraceabilityScore.ScoreType.SOME_COUNTRIES));
					}
					else
					{
						scores.Add(new TraceabilityScore(TraceabilityScore.ScoreType.ALL_COUNTRIES));
					}
				}
			}

			if (regionsCount > 0)
			{
				// All countries have region
				if (regionsCount == countryCount && onlyContinentCount == 0)
				{
					scores.Add(new TraceabilityScore(TraceabilityScore.ScoreType.ALL_REGIONS));
				}
				// Some regions
				else
				{
					scores.Add(new TraceabilityScore(TraceabilityScore.ScoreType.SOME_REGIONS));
				}
			}

			// Producer Basic +3
			if (bean.HasProducerInfo)
			{
				scores.Add(new TraceabilityScore(TraceabilityScore.ScoreType.HAS_PRODUCER));
			}

			// Importer Name +3 Or direct trade since there is no importer
			if (bean.HasImporterName )
			{
				scores.Add(new TraceabilityScore(TraceabilityScore.ScoreType.HAS_IMPORTER));
			}

			if(bean.IsDirectTradeCertified)
			{
				scores.Add(new TraceabilityScore(TraceabilityScore.ScoreType.DIRECT_TRADE));
			}

			// Processor Name +3
			if (bean.HasProcessorName)
			{
				scores.Add(new TraceabilityScore(TraceabilityScore.ScoreType.HAS_PROCESSOR));
			}

			return scores;
		}
	}

	public class TraceabilityScore
	{
		public int ScoreModifier { get; set; }
		public string ScoreNote { get; set; } = String.Empty;
		public ScoreType ScType;
		public enum ScoreType
		{
			NO_SOURCING,
			ONLY_CONTINENTS,
			SOME_COUNTRIES,
			ALL_COUNTRIES,
			SOME_REGIONS,
			ALL_REGIONS,
			HAS_PRODUCER,
			HAS_IMPORTER,
			DIRECT_TRADE,
			HAS_PROCESSOR
		}

		public TraceabilityScore(ScoreType type)
		{
			ScType = type;
			ScoreModifier = GetScoreModifier(type);
			ScoreNote = GetScoreNote(type);
		}

		public static string GetScoreNote(ScoreType type)
		{
			int modifier = GetScoreModifier(type);
			string modifierText = modifier > 0 ? "+" + modifier : modifier.ToString();

			switch (type)
			{ 
				case ScoreType.NO_SOURCING:
					return "<br/><span>No sources " + modifierText + "</span>";
				case ScoreType.ONLY_CONTINENTS:
					return "<br/><span>No origin countries " + modifierText + "</span>";
				case ScoreType.SOME_COUNTRIES:
					return "<br/><span>Some origin countries " + modifierText + "</span>";
				case ScoreType.ALL_COUNTRIES:
					return "<br/><span>Has origin countries " + modifierText + "</span>";
				case ScoreType.SOME_REGIONS:
					return "<br/><span>Some origin regions " + modifierText + "</span>";
				case ScoreType.ALL_REGIONS:
					return "<br/><span>Has origin regions " + modifierText + "</span>";
				case ScoreType.HAS_PRODUCER:
					return "<br/><span>Has Producer " + modifierText + "</span>";
				case ScoreType.HAS_IMPORTER:
					return "<br/><span>Has Importer " + modifierText + "</span>";
				case ScoreType.DIRECT_TRADE:
					return "<br/><span>Direct Trade " + modifierText + "</span>";
				case ScoreType.HAS_PROCESSOR:
					return "<br/><span>Has Processor " + modifierText + "</span>";
				default:
					return "";
			}
		}

		private static int GetScoreModifier(ScoreType type)
		{
			switch (type)
			{
				case ScoreType.NO_SOURCING:
					return -5;
				case ScoreType.ONLY_CONTINENTS:
					return -3;
				case ScoreType.SOME_COUNTRIES:
					return -2;
				case ScoreType.SOME_REGIONS:
					return 2;
				case ScoreType.ALL_COUNTRIES:
				case ScoreType.ALL_REGIONS:
				case ScoreType.HAS_PRODUCER:
				case ScoreType.HAS_IMPORTER:
				case ScoreType.HAS_PROCESSOR:
				case ScoreType.DIRECT_TRADE:
					return 3;
				default:
					return 0;
			}
		}
	}
}
