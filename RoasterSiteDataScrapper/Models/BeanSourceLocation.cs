using static RoasterBeansDataAccess.Models.BeanOrigin;

namespace RoasterBeansDataAccess.Models
{
	public class SourceLocation
	{
		public string? City { get; set; }
		public string? Region { get; set; }
		public SourceCountry Country { get; set; } = SourceCountry.UNKNOWN;
		public SourceContinent? Continent { get; set; }

		public SourceLocation()
		{

		}

		public SourceLocation(string? city, string? region, SourceCountry country)
		{
			City = city;
			Region = region;
			Country = country;
			Continent = GetContinentFromCountry(country);
		}

		public SourceLocation(SourceCountry country)
		{
			Country = country;
			Continent = GetContinentFromCountry(country);
		}

		public SourceLocation(SourceContinent continent)
		{
			Continent = continent;
		}

		public static SourceContinent? GetContinentFromCountry(SourceCountry country)
		{
			switch (country)
			{
				case SourceCountry.ETHIOPIA:
				case SourceCountry.RWANDA:
				case SourceCountry.KENYA:
				case SourceCountry.UGANDA:
				case SourceCountry.BURUNDI:
				case SourceCountry.DEMOCRATIC_REPUBLIC_OF_THE_CONGO:
				case SourceCountry.TANZANIA:
					return SourceContinent.AFRICA;
				case SourceCountry.COLOMBIA:
				case SourceCountry.BRAZIL:
				case SourceCountry.PERU:
				case SourceCountry.ECUADOR:
					return SourceContinent.SOUTH_AMERICA;
				case SourceCountry.GUATEMALA:
				case SourceCountry.EL_SALVADOR:
				case SourceCountry.HONDURAS:
				case SourceCountry.NICARAGUA:
				case SourceCountry.MEXICO:
				case SourceCountry.COSTA_RICA:
				case SourceCountry.DOMINICAN_REPUBLIC:
				case SourceCountry.HAITI:
				case SourceCountry.BOLIVIA:
					return SourceContinent.CENTRAL_AMERICA;
				case SourceCountry.INDONESIA:
				case SourceCountry.PAPUA_NEW_GUINEA:
				case SourceCountry.EAST_TIMOR:
				case SourceCountry.VIETNAM:
				case SourceCountry.CHINA:
				case SourceCountry.MYANMAR:
				case SourceCountry.THAILAND:
				case SourceCountry.PHILIPPINES:
					return SourceContinent.ASIA;
			}

			return null;
		}

        public static string GetIso2AlphaCode(SourceCountry country)
        {
            switch (country)
            {
                case SourceCountry.ETHIOPIA:
                    return "ET";
                case SourceCountry.RWANDA:
                    return "RW";
                case SourceCountry.KENYA:
                    return "KE";
                case SourceCountry.UGANDA:
                    return "UG";
                case SourceCountry.BURUNDI:
                    return "BI";
                case SourceCountry.DEMOCRATIC_REPUBLIC_OF_THE_CONGO:
                    return "CD";
                case SourceCountry.TANZANIA:
                    return "TZ";
                case SourceCountry.COLOMBIA:
                    return "CO";
                case SourceCountry.BRAZIL:
                    return "BR";
                case SourceCountry.PERU:
                    return "PE";
                case SourceCountry.ECUADOR:
                    return "EC";
                case SourceCountry.GUATEMALA:
                    return "GT";
                case SourceCountry.EL_SALVADOR:
                    return "SV";
                case SourceCountry.HONDURAS:
                    return "HN";
                case SourceCountry.NICARAGUA:
                    return "NI";
                case SourceCountry.MEXICO:
                    return "MX";
                case SourceCountry.COSTA_RICA:
                    return "CR";
                case SourceCountry.DOMINICAN_REPUBLIC:
                    return "DO";
                case SourceCountry.HAITI:
                    return "HT";
                case SourceCountry.BOLIVIA:
                    return "BO";
                case SourceCountry.INDONESIA:
                    return "ID";
                case SourceCountry.PAPUA_NEW_GUINEA:
                    return "PG";
                case SourceCountry.EAST_TIMOR:
                    return "TL";
                case SourceCountry.VIETNAM:
                    return "VN";
                case SourceCountry.CHINA:
                    return "CN";
                case SourceCountry.MYANMAR:
                    return "MM";
                case SourceCountry.THAILAND:
                    return "TH";
                case SourceCountry.PHILIPPINES:
                    return "PH";
				default:
                    return string.Empty;
            }
        }
	}
}
