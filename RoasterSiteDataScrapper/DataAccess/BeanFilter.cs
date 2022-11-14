using RoasterBeansDataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public FilterValueBool IsSingleOrigin { get; set; } = new FilterValueBool(false, false);
        public FilterValueBool IsDecaf { get; set; } = new FilterValueBool(false, false);
        public FilterValueBool IsFairTradeCertified { get; set; } = new FilterValueBool(false, false);
        public FilterValueBool IsDirectTradeCertified { get; set; } = new FilterValueBool(false, false);
		public FilterValueBool IsInStock { get; set; } = new FilterValueBool(false, false);
		public FilterValueBool AvailablePreground { get; set; } = new FilterValueBool(false, false);
		public FilterList<Country> CountryFilter { get; set; } = new FilterList<Country>(false, new List<Country>());
        public FilterList<RoastLevel> RoastFilter { get; set; } = new FilterList<RoastLevel>(false, new List<RoastLevel>());
        public FilterList<BeanProcessing> ProcessFilter { get; set; } = new FilterList<BeanProcessing>(false, new List<BeanProcessing>());
        public FilterList<OrganicCerification> OrganicFilter { get; set; } = new FilterList<OrganicCerification>(false, new List<OrganicCerification>());
        public FilterSearchString SearchNameString { get; set; } = new FilterSearchString(false, "");
        public FilterSearchString SearchTastingNotesString { get; set; } = new FilterSearchString(false, "");

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
            if (!IsActive || compareTo.ToLower().Contains(CompareString))
            {
                return true;
            }

            return false;
        }

        public bool MatchesFilter(List<string> compareTo)
        {
            if (IsActive)
            {
                foreach (string compareToString in compareTo)
                {
                    if (compareToString.ToLower().Contains(CompareString))
                    {
                        return true;
                    }
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
