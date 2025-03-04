using RoasterBeansDataAccess.Models;
using SeattleRoasterProject.Core.Enums;

namespace RoasterBeansDataAccess.Services;

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
        var scores = GetScoresForOrigins(bean);
        return scores.Sum(s => s.ScoreModifier);
    }

    public static string GetScoreBreakdownDisplay(BeanModel bean)
    {
        var scores = GetScoresForOrigins(bean);
        var totalScore = scores.Sum(s => s.ScoreModifier);

        var displayText = "<b>Traceability Score: " + totalScore + "</b>";
        foreach (var score in scores)
        {
            displayText += TraceabilityScore.GetScoreNote(score.ScType);
        }

        return displayText;
    }

    public static string GetScoreStarDisplay(BeanModel bean)
    {
        var score = GetTotalScore(bean);

        var starHalves = 0;
        if (score <= -5)
        {
            starHalves = 0;
        }
        else if (score <= -2)
        {
            starHalves = 1;
        }
        else if (score <= 0)
        {
            starHalves = 2;
        }
        else if (score <= 2)
        {
            starHalves = 3;
        }
        else if (score <= 3)
        {
            starHalves = 4;
        }
        else if (score <= 5)
        {
            starHalves = 5;
        }
        else if (score <= 6)
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

        var result = "";
        for (var i = 0; i < 5; i++)
        {
            if (i < starHalves / 2)
            {
                result += "<span class=\"bi bi-star-fill\"></span>";
            }
            else
            {
                if (starHalves / 2 == i && starHalves % 2 == 1)
                {
                    result += "<span class=\"bi bi-star-half\"></span>";
                }
                else
                {
                    result += "<span class=\"bi bi-star\"></span>";
                }
            }
        }

        return result;
    }

    private static List<TraceabilityScore> GetScoresForOrigins(BeanModel bean)
    {
        List<TraceabilityScore> scores = new();

        var regionsCount = 0;
        var countryCount = 0;
        var onlyContinentCount = 0;

        if (bean.Origins != null)
        {
            regionsCount = bean.Origins.Count(o => !string.IsNullOrEmpty(o.City) || !string.IsNullOrEmpty(o.Region));
            countryCount = bean.Origins.Count(o => o.Country != SourceCountry.Unknown);
            onlyContinentCount = bean.Origins.Count(o => o.Continent != null && o.Country == SourceCountry.Unknown);
        }

        // No sourcing information
        if (onlyContinentCount == 0 && countryCount == 0)
        {
            scores.Add(new TraceabilityScore(ScoreType.No_Sourcing));
        }
        else
        {
            // Only continents
            if (countryCount == 0)
            {
                scores.Add(new TraceabilityScore(ScoreType.Only_Continents));
            }
            // Some countries
            else
            {
                if (onlyContinentCount > 0)
                {
                    scores.Add(new TraceabilityScore(ScoreType.Some_Countries));
                }
                else
                {
                    scores.Add(new TraceabilityScore(ScoreType.All_Countries));
                }
            }
        }

        if (regionsCount > 0)
        {
            // All countries have region
            if (regionsCount == countryCount && onlyContinentCount == 0)
            {
                scores.Add(new TraceabilityScore(ScoreType.All_Regions));
            }
            // Some regions
            else
            {
                scores.Add(new TraceabilityScore(ScoreType.Some_Regions));
            }
        }

        // Producer Basic +3
        if (bean.HasProducerInfo)
        {
            scores.Add(new TraceabilityScore(ScoreType.Has_Producer));
        }

        // Importer Name +3 Or direct trade since there is no importer
        if (bean.HasImporterName)
        {
            scores.Add(new TraceabilityScore(ScoreType.Has_Importer));
        }

        if (bean.IsDirectTradeCertified)
        {
            scores.Add(new TraceabilityScore(ScoreType.Direct_Trade));
        }

        // Processor Name +3
        if (bean.HasProcessorName)
        {
            scores.Add(new TraceabilityScore(ScoreType.Has_Processor));
        }

        return scores;
    }
}

public class TraceabilityScore
{
    public ScoreType ScType;


    public TraceabilityScore(ScoreType type)
    {
        ScType = type;
        ScoreModifier = GetScoreModifier(type);
        ScoreNote = GetScoreNote(type);
    }

    public int ScoreModifier { get; set; }
    public string ScoreNote { get; set; } = string.Empty;

    public static string GetScoreNote(ScoreType type)
    {
        var modifier = GetScoreModifier(type);
        var modifierText = modifier > 0 ? "+" + modifier : modifier.ToString();

        switch (type)
        {
            case ScoreType.No_Sourcing:
                return "<br/><span>No sources " + modifierText + "</span>";
            case ScoreType.Only_Continents:
                return "<br/><span>No origin countries " + modifierText + "</span>";
            case ScoreType.Some_Countries:
                return "<br/><span>Some origin countries " + modifierText + "</span>";
            case ScoreType.All_Countries:
                return "<br/><span>Has origin countries " + modifierText + "</span>";
            case ScoreType.Some_Regions:
                return "<br/><span>Some origin regions " + modifierText + "</span>";
            case ScoreType.All_Regions:
                return "<br/><span>Has origin regions " + modifierText + "</span>";
            case ScoreType.Has_Producer:
                return "<br/><span>Has Producer " + modifierText + "</span>";
            case ScoreType.Has_Importer:
                return "<br/><span>Has Importer " + modifierText + "</span>";
            case ScoreType.Direct_Trade:
                return "<br/><span>Direct Trade " + modifierText + "</span>";
            case ScoreType.Has_Processor:
                return "<br/><span>Has Processor " + modifierText + "</span>";
            default:
                return "";
        }
    }

    private static int GetScoreModifier(ScoreType type)
    {
        switch (type)
        {
            case ScoreType.No_Sourcing:
                return -5;
            case ScoreType.Only_Continents:
                return -3;
            case ScoreType.Some_Countries:
                return -2;
            case ScoreType.Some_Regions:
                return 2;
            case ScoreType.All_Countries:
            case ScoreType.All_Regions:
            case ScoreType.Has_Producer:
            case ScoreType.Has_Importer:
            case ScoreType.Has_Processor:
            case ScoreType.Direct_Trade:
                return 3;
            default:
                return 0;
        }
    }
}