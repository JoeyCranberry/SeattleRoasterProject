using SeattleRoasterProject.Core.Enums;

namespace RoasterBeansDataAccess.DataAccess;

/*
 * BeanFilter is made up FilterValues which can be set as active or inactive
 * and compared against a value to see if the filter should allow that value.
 *
 * For example, if the filter, IsSingleOrigin is turned on, and the compareValue is true -
 * only beans with IsSingleOrigin == true will be returned
 */
public class BeanFilter
{
    public FilterValueBool IsExcluded { get; set; } = new(false, false);
    public FilterList<string> ValidRoasters { get; set; } = new(false, new List<string>());
    public FilterList<string> ChosenRoasters { get; set; } = new(false, new List<string>());
    public FilterValueBool IsSingleOrigin { get; set; } = new(false, false);
    public FilterValueBool IsDecaf { get; set; } = new(false, false);
    public FilterValueBool IsFairTradeCertified { get; set; } = new(false, false);
    public FilterValueBool IsDirectTradeCertified { get; set; } = new(false, false);
    public FilterValueBool IsInStock { get; set; } = new(false, false);
    public FilterValueBool AvailablePreground { get; set; } = new(false, false);
    public FilterValueBool IsSupportingCause { get; set; } = new(false, false);
    public FilterValueBool IsFromWomanOwnedFarms { get; set; } = new(false, false);
    public FilterValueBool IsRainforestAllianceCertified { get; set; } = new(false, false);
    public FilterList<SourceCountry> CountryFilter { get; set; } = new(false, new List<SourceCountry>());
    public FilterList<RoastLevel> RoastFilter { get; set; } = new(false, new List<RoastLevel>());
    public FilterList<ProcessingMethod> ProcessFilter { get; set; } = new(false, new List<ProcessingMethod>());
    public FilterList<OrganicCertification> OrganicFilter { get; set; } = new(false, new List<OrganicCertification>());
    public FilterSearchString SearchNameString { get; set; } = new(false, "");
    public FilterSearchString SearchTastingNotesString { get; set; } = new(false, "");
    public FilterList<string> RoasterNameSearch { get; set; } = new(false, new List<string>());
    public FilterList<string> RegionFilter { get; set; } = new(false, new List<string>());
    public FilterList<string> TastingNotesFilter { get; set; } = new(false, new List<string>());
    public FilterValueBool IsActiveListing { get; set; } = new(false, false);
}

public class FilterValueBool
{
    public FilterValueBool(bool isActive, bool compareValue)
    {
        IsActive = isActive;
        CompareValue = compareValue;
    }

    public bool IsActive { get; set; }
    public bool CompareValue { get; set; }

    public bool MatchesFilter(bool valueToCompare)
    {
        if (!IsActive || valueToCompare == CompareValue)
        {
            return true;
        }

        return false;
    }
}

public class FilterList<T>
{
    public FilterList(bool isActive, List<T> values)
    {
        IsActive = isActive;
        CompareValues = values;
    }

    public bool IsActive { get; set; }
    public List<T> CompareValues { get; set; }

    public bool MatchesFilter(List<T> valuesToCompare)
    {
        if (IsActive)
        {
            if (valuesToCompare == null)
            {
                return false;
            }

            if (CompareValues.Intersect(valuesToCompare).Any())
            {
                return true;
            }
        }
        else
        {
            return true;
        }

        return false;
    }

    public bool MatchesFilter(T valueToCompare)
    {
        if (!IsActive || CompareValues.Contains(valueToCompare))
        {
            return true;
        }

        return false;
    }
}

public class FilterSearchString
{
    public FilterSearchString(bool isActive, string compareString)
    {
        IsActive = isActive;
        CompareString = compareString.Trim().ToLower();
    }

    public bool IsActive { get; set; }
    public string CompareString { get; set; }

    public bool MatchesFilter(string compareTo)
    {
        compareTo = compareTo.ToLower();

        if (IsActive)
        {
            return MatchesFilter(compareTo.Split(' ').ToList());
        }

        return false;
    }

    public bool MatchesFilter(List<string> compareTo)
    {
        if (IsActive)
        {
            var compareStringSplit = CompareString.Split(' ').ToList();
            for (var i = 0; i < compareTo.Count; i++)
            {
                compareTo[i] = compareTo[i].ToLower();
            }

            if (compareTo.Intersect(compareStringSplit).Any())
            {
                return true;
            }
        }
        else
        {
            return true;
        }

        return false;
    }
}