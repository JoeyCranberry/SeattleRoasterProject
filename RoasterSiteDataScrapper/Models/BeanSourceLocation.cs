using SeattleRoasterProject.Core.Enums;

namespace RoasterBeansDataAccess.Models;

public class SourceLocation
{
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

    public string? City { get; set; }
    public string? Region { get; set; }
    public SourceCountry Country { get; set; } = SourceCountry.Unknown;
    public SourceContinent? Continent { get; set; }

    public static SourceContinent? GetContinentFromCountry(SourceCountry country)
    {
        switch (country)
        {
            case SourceCountry.Ethiopia:
            case SourceCountry.Rwanda:
            case SourceCountry.Kenya:
            case SourceCountry.Uganda:
            case SourceCountry.Burundi:
            case SourceCountry.Democratic_Republic_Of_The_Congo:
            case SourceCountry.Tanzania:
                return SourceContinent.Africa;
            case SourceCountry.Colombia:
            case SourceCountry.Brazil:
            case SourceCountry.Peru:
            case SourceCountry.Ecuador:
                return SourceContinent.South_America;
            case SourceCountry.Guatemala:
            case SourceCountry.El_Salvador:
            case SourceCountry.Honduras:
            case SourceCountry.Nicaragua:
            case SourceCountry.Mexico:
            case SourceCountry.Costa_Rica:
            case SourceCountry.Dominican_Republic:
            case SourceCountry.Haiti:
            case SourceCountry.Bolivia:
                return SourceContinent.Central_America;
            case SourceCountry.Indonesia:
            case SourceCountry.Papua_New_Guinea:
            case SourceCountry.East_Timor:
            case SourceCountry.Vietnam:
            case SourceCountry.China:
            case SourceCountry.Myanmar:
            case SourceCountry.Thailand:
            case SourceCountry.Philippines:
                return SourceContinent.Asia;
        }

        return null;
    }

    public static string GetIso2AlphaCode(SourceCountry country)
    {
        switch (country)
        {
            case SourceCountry.Ethiopia:
                return "ET";
            case SourceCountry.Rwanda:
                return "RW";
            case SourceCountry.Kenya:
                return "KE";
            case SourceCountry.Uganda:
                return "UG";
            case SourceCountry.Burundi:
                return "BI";
            case SourceCountry.Democratic_Republic_Of_The_Congo:
                return "CD";
            case SourceCountry.Tanzania:
                return "TZ";
            case SourceCountry.Colombia:
                return "CO";
            case SourceCountry.Brazil:
                return "BR";
            case SourceCountry.Peru:
                return "PE";
            case SourceCountry.Ecuador:
                return "EC";
            case SourceCountry.Guatemala:
                return "GT";
            case SourceCountry.El_Salvador:
                return "SV";
            case SourceCountry.Honduras:
                return "HN";
            case SourceCountry.Nicaragua:
                return "NI";
            case SourceCountry.Mexico:
                return "MX";
            case SourceCountry.Costa_Rica:
                return "CR";
            case SourceCountry.Dominican_Republic:
                return "DO";
            case SourceCountry.Haiti:
                return "HT";
            case SourceCountry.Bolivia:
                return "BO";
            case SourceCountry.Indonesia:
                return "ID";
            case SourceCountry.Papua_New_Guinea:
                return "PG";
            case SourceCountry.East_Timor:
                return "TL";
            case SourceCountry.Vietnam:
                return "VN";
            case SourceCountry.China:
                return "CN";
            case SourceCountry.Myanmar:
                return "MM";
            case SourceCountry.Thailand:
                return "TH";
            case SourceCountry.Philippines:
                return "PH";
            default:
                return string.Empty;
        }
    }
}