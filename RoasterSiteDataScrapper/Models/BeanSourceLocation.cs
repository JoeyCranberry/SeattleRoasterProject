using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
					return SourceContinent.CENTRAL_AMERICA;
				case SourceCountry.INDONESIA:
				case SourceCountry.PAPAU_NEW_GUINEA:
				case SourceCountry.EAST_TIMOR:
				case SourceCountry.VIETNAM:
				case SourceCountry.CHINA:
				case SourceCountry.MYANMAR:
				case SourceCountry.THAILAND:
					return SourceContinent.ASIA;
			}

			return null;
		}
	}
}
