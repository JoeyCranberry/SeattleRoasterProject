using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoasterBeansDataAccess
{
    public class Roaster
    {
        public int RoasterId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public RoasterLocation Location { get; set; }
        public string ShopURL { get; set; }
        public string ImageURL { get; set; }
        public string ImageClass { get; set; }
        public int FoundingYear { get; set; }
    }

    public enum RoasterLocation
    { 
        SEATTLE,
        KIRKLAND,
        OLYMPIA,
        BELLINGHAM,
        TACOMA,
        RENTON,
        GEORGETOWN,
        GIG_HARBOR,
        BELLEVUE,
        SUMNER,
        BAINBRIDGE,
        KENT,
        EVERETT,
        REDMOND,
        SNOHOMISH,
        SHORELINE
    }
}
