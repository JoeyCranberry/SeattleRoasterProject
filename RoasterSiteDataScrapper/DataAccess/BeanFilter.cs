using RoasterBeansDataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RoasterBeansDataAccess.Models.BeanOrigin;

namespace RoasterBeansDataAccess.DataAccess
{
    /*
     * BeanFilter is made up FilterValues which can be set as active or inactive 
     * and compared against a value to see if the filter should allow that value. 
     * 
     * For example, if the filter, IsSingleOrigin is turned on, and the compareValue is true - 
     * only beans with IsSingleOrigin == true will be returned
     */
    public class BeanFilter
    {
        public FilterValueBool IsExcluded { get; set; } = new FilterValueBool(false, false);
		public FilterList<string> ValidRoasters { get; set; } = new FilterList<string>(false, new List<string>());
		public FilterList<string> ChosenRoasters { get; set; } = new FilterList<string>(false, new List<string>());
		public FilterValueBool IsSingleOrigin { get; set; } = new FilterValueBool(false, false);
        public FilterValueBool IsDecaf { get; set; } = new FilterValueBool(false, false);
        public FilterValueBool IsFairTradeCertified { get; set; } = new FilterValueBool(false, false);
        public FilterValueBool IsDirectTradeCertified { get; set; } = new FilterValueBool(false, false);
		public FilterValueBool IsInStock { get; set; } = new FilterValueBool(false, false);
		public FilterValueBool AvailablePreground { get; set; } = new FilterValueBool(false, false);
        public FilterValueBool IsSupportingCause { get; set; } = new FilterValueBool(false, false);
		public FilterValueBool IsFromWomanOwnedFarms { get; set; } = new FilterValueBool(false, false);
		public FilterValueBool IsRainforestAllianceCertified { get; set; } = new FilterValueBool(false, false);
		public FilterList<SourceCountry> CountryFilter { get; set; } = new FilterList<SourceCountry>(false, new List<SourceCountry>());
        public FilterList<RoastLevel> RoastFilter { get; set; } = new FilterList<RoastLevel>(false, new List<RoastLevel>());
        public FilterList<ProcessingMethod> ProcessFilter { get; set; } = new FilterList<ProcessingMethod>(false, new List<ProcessingMethod>());
        public FilterList<OrganicCerification> OrganicFilter { get; set; } = new FilterList<OrganicCerification>(false, new List<OrganicCerification>());
        public FilterSearchString SearchNameString { get; set; } = new FilterSearchString(false, "");
        public FilterSearchString SearchTastingNotesString { get; set; } = new FilterSearchString(false, "");
        public FilterList<string> RoasterNameSearch { get; set; } = new FilterList<string>(false, new List<string>());
        public FilterList<string> RegionFilter { get; set; } = new FilterList<string>(false, new List<string>());
        public FilterList<string> TastingNotesFilter { get; set; } = new FilterList<string>(false, new List<string>());
		public FilterValueBool IsActiveListing { get; set; } = new FilterValueBool(false, false);
	}

    public class FilterValueBool
    {
        public bool IsActive { get; set; } = false;
        public bool CompareValue { get; set; }

        public FilterValueBool(bool isActive, bool compareValue)
        {
            IsActive = isActive;
            CompareValue = compareValue;
        }

        public bool MatchesFilter(bool valueToCompare)
        {
            if(!IsActive || valueToCompare == CompareValue)
            {
                return true;
            }
            else
            {
                return false;
            }    
        }
    }

    public class FilterList<T>
    {
        public bool IsActive { get; set; } = false;
        public List<T> CompareValues { get; set; }

        public FilterList(bool isActive, List<T> values)
        {
            IsActive = isActive;
            CompareValues = values;
        }

        public bool MatchesFilter(List<T> valuesToCompare)
        {
            if (IsActive)
            {
                if(valuesToCompare == null)
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
            else
            {
                return false;
            }
        }
    }

    public class FilterSearchString
    {
        public bool IsActive { get; set; }
        public string CompareString { get; set; } 

        public FilterSearchString(bool isActive, string compareString) 
        {
            IsActive = isActive;
            CompareString = compareString.Trim().ToLower();
        }
        
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
				List<string> compareStringSplit = CompareString.Split(' ').ToList();
				for (int i = 0; i < compareTo.Count; i++)
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
}
