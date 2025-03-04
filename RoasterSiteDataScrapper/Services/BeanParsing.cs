using SeattleRoasterProject.Core.Enums;

namespace RoasterBeansDataAccess.Services;

public class BeanNameParsing
{
    public static List<SourceCountry> GetCountriesFromName(string beanName)
    {
        var countriesFromName = new List<SourceCountry>();

        foreach (var country in Enum.GetValues<SourceCountry>())
        {
            if (beanName.ToLower().Contains(country.ToString().ToLower().Replace("_", " ")))
            {
                countriesFromName.Add(country);
            }
        }

        if (beanName.ToLower().Contains("sumatra") && !countriesFromName.Contains(SourceCountry.Indonesia))
        {
            countriesFromName.Add(SourceCountry.Indonesia);
        }

        if (beanName.ToLower().Contains("congo") &&
            !countriesFromName.Contains(SourceCountry.Democratic_Republic_Of_The_Congo))
        {
            countriesFromName.Add(SourceCountry.Democratic_Republic_Of_The_Congo);
        }

        return countriesFromName;
    }

    public static List<ProcessingMethod> GetProcessFromName(string beanName)
    {
        var results = new List<ProcessingMethod>();

        foreach (var process in Enum.GetValues<ProcessingMethod>())
        {
            if (beanName.ToLower().Contains(process.ToString().ToLower().Replace("_", " ")))
            {
                results.Add(process);
            }
        }

        return results;
    }

    public static bool GetIsDecafFromName(string beanName)
    {
        if (beanName.ToLower().Contains("decaf"))
        {
            return true;
        }

        return false;
    }

    public static OrganicCertification GetOrganicFromName(string beanName)
    {
        if (beanName.ToLower().Contains("organic") || beanName.ToLower().Contains("fto"))
        {
            if (beanName.ToLower().Contains("usda"))
            {
                return OrganicCertification.Certified_Organic;
            }

            return OrganicCertification.Uncertified_Organic;
        }

        return OrganicCertification.Not_Organic;
    }

    public static RoastLevel SetRoastLevelFromName(string beanName)
    {
        foreach (var roast in Enum.GetValues<RoastLevel>())
        {
            if (beanName.ToLower().Contains(roast.ToString().ToLower().Replace("_", " ")))
            {
                return roast;
            }
        }

        return RoastLevel.Unknown;
    }

    public static bool SetIsFairTradeFromName(string beanName)
    {
        if (beanName.ToLower().Contains("fair-trade") || beanName.ToLower().Contains("fair trade") ||
            beanName.ToLower().Contains("fto"))
        {
            return true;
        }

        return false;
    }
}