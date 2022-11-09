using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoasterBeansDataAccess
{
    public class BeanListing
    {
        public string FullName { get; set; }
        public Roaster Roaster { get; set; }
        public string DateAdded { get; set; }
        public string ImageURL { get; set; }
        public decimal PriceBeforeShipping { get; set; }
        public BeanProcessing ProcessingMethod { get; set; }
        public RoastLevel RoastLevel { get; set; }
        public List<Country> CountriesOfOrigin { get; set; }
        public bool IsFairTradeCertified { get; set; }
        public bool IsDirectTradeCertified { get; set; }
        public OrganicCerification OrganicCerification { get; set; }
        public List<string> TastingNotes { get; set; }
        public bool IsSingleOrigin { get; set; }

        // Country of Origin, Roaster
    }

    public enum BeanProcessing
    {
        NATURAL,
        WASHED,
        FERMETTED
    }

    public enum RoastLevel
    { 
        LIGHT,
        MEDIUM,
        DARK
    }

    public enum OrganicCerification
    {
        USDA_ORGANIC,
        UNCERTIFIED_ORGANIC,
        NOT_ORGANIC
    }

    public enum Country
    { 
        ETHIOPIA,
        COLUMBIA,
        RWANDA,
        GUATEMALA,
        EL_SALVADOR,
        INDONESIA,
        HONDURAS,
        NICARAGUA
    }
}
