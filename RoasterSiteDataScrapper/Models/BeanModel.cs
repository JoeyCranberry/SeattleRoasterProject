using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization.Attributes;

namespace RoasterBeansDataAccess.Models
{
    public class BeanModel
    {
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string Id { get; set; }
        public string FullName { get; set; }
        public int RoasterId { get; set; }
        public string MongoRoasterId { get; set; }
        public DateTime DateAdded { get; set; }
        public string ProductURL { get; set; }
        public string ImageURL { get; set; }
        public string ImageClass { get; set; }
        public decimal PriceBeforeShipping { get; set; }
        public BeanProcessing ProcessingMethod { get; set; }
        public RoastLevel RoastLevel { get; set; }
        public List<Country> CountriesOfOrigin { get; set; }
        public bool IsFairTradeCertified { get; set; }
        public bool IsDirectTradeCertified { get; set; }
        public OrganicCerification OrganicCerification { get; set; }
        public List<string> TastingNotes { get; set; }
        public bool IsSingleOrigin { get; set; }
        public bool IsDecaf { get; set; }
        public bool IsExcluded { get; set; } = false;

        public void SetOriginsFromName()
        {
            List<Country> countriesFromName = new List<Country>();

            foreach (var country in Enum.GetValues<Country>())
            {
                if (FullName.ToLower().Contains(country.ToString().ToLower().Replace("_", " ")))
                {
                    countriesFromName.Add(country);
                }
            }

            if (FullName.ToLower().Contains("sumatra"))
            {
                countriesFromName.Add(Country.INDONESIA);
            }

            if (countriesFromName.Count > 0)
            {
                CountriesOfOrigin = countriesFromName;

                if (countriesFromName.Count == 1 && countriesFromName[0] != Country.UNKNOWN)
                {
                    IsSingleOrigin = true;
                }
                else
                {
                    IsSingleOrigin = false;
                }
            }

            if (FullName.ToLower().Contains("blend"))
            {
                IsSingleOrigin = false;
            }
        }

        public void SetProcessFromName()
        {
            foreach (var process in Enum.GetValues<BeanProcessing>())
            {
                if (FullName.ToLower().Contains(process.ToString().ToLower().Replace("_", " ")))
                {
                    ProcessingMethod = process;
                }
            }
        }

        public void SetDecafFromName()
        {
            if (FullName.ToLower().Contains(value: "decaf"))
            {
                IsDecaf = true;
            }
        }

        public void SetOrganicFromName()
        {
            if (FullName.ToLower().Contains(value: "organic"))
            {
                if (FullName.ToLower().Contains(value: "usda"))
                {
                    OrganicCerification = OrganicCerification.CERTIFIED_ORGANIC;
                }
                else
                {
                    OrganicCerification = OrganicCerification.UNCERTIFIED_ORGANIC;
                }
            }
        }

        public void SetRoastLevelFromName()
        {
            foreach (var roast in Enum.GetValues<RoastLevel>())
            {
                if (FullName.ToLower().Contains(roast.ToString().ToLower().Replace("_", " ")))
                {
                    RoastLevel = roast;
                }
            }
        }

        // Converts the Country enum into title case and optionally adds flag emoji
        public string GetCountryDisplayName(Country country, bool includeFlag = false)
        {
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            string titleCase = textInfo.ToTitleCase(country.ToString().ToLower().Replace("_", ""));

            if(includeFlag)
            {
                switch(country)
                {
                    case Country.ETHIOPIA:
                        titleCase = "🇪🇹 " + titleCase;
                        break;
                    case Country.COLOMBIA:
                        titleCase = "🇨🇴 " + titleCase;
                        break;
                    case Country.RWANDA:
                        titleCase = "🇪🇹 " + titleCase;
                        break;
                    case Country.GUATEMALA:
                        titleCase = "🇷🇼 " + titleCase;
                        break;
                    case Country.EL_SALVADOR:
                        titleCase = "🇸🇻 " + titleCase;
                        break;
                    case Country.INDONESIA:
                        titleCase = "🇮🇩 " + titleCase;
                        break;
                    case Country.HONDURAS:
                        titleCase = "🇭🇳 " + titleCase;
                        break;
                    case Country.NICARAGUA:
                        titleCase = "🇳🇮 " + titleCase;
                        break;
                    case Country.BRAZIL:
                        titleCase = "🇧🇷 " + titleCase;
                        break;
                    case Country.KENYA:
                        titleCase = "🇰🇪 " + titleCase;
                        break;
                }
            }
            
            return titleCase;
        }

        public static string GetCountryPossesiveTerm(Country country)
        {
			switch (country)
			{
				case Country.ETHIOPIA:
                    return "Ethiopian";
				case Country.COLOMBIA:
					return "Columbian";
				case Country.RWANDA:
					return "Rwandan";
				case Country.GUATEMALA:
					return "Guatemalian";
				case Country.EL_SALVADOR:
					return "El Salvadorian";
				case Country.INDONESIA:
					return "Indonesian";
				case Country.HONDURAS:
					return "Honduran";
				case Country.NICARAGUA:
					return "Nicaraguan";
				case Country.BRAZIL:
					return "Brazilian";
				case Country.KENYA:
					return "Kenyan";
                default:
                    return country.ToString();
			}
		}
    }

    public enum BeanProcessing
    {
        UNKNOWN,
        NATURAL,
        HONEY,
        WASHED,
        WET_HULLED
    }

    public enum RoastLevel
    {
        UNKNOWN,
        LIGHT,
        MEDIUM,
        DARK
    }

    public enum OrganicCerification
    {
        NOT_ORGANIC,
        CERTIFIED_ORGANIC,
        UNCERTIFIED_ORGANIC
    }

    public enum Country
    {
        UNKNOWN,
        ETHIOPIA,
        COLOMBIA,
        RWANDA,
        GUATEMALA,
        EL_SALVADOR,
        INDONESIA,
        HONDURAS,
        NICARAGUA,
        BRAZIL,
        KENYA
    }
}
