using SeattleRoasterProject.Core.Enums;
using SeattleRoasterProject.Data.Enums;
using SeattleRoasterProject.Data.Models;

namespace SeattleRoasterProject.Data.Services;

public class BeanSortingService
{
    public IEnumerable<BeanListingModel> SortBeanListings(IEnumerable<BeanListingModel> unsorted, SortMethod method)
    {
        switch (method.SortByField)
        {
            default:
            case SortField.Default:
                return SortBeanListingsDefault(unsorted);
            case SortField.Price:
                return SortBeanListingsPrice(unsorted, method.IsLowToHigh);
            case SortField.Date_Added:
                return SortBeanListingsDateAdded(unsorted, method.IsLowToHigh);
            case SortField.Alphabetical:
                return SortBeanListingsAlphabetical(unsorted, method.IsLowToHigh);
            case SortField.Roaster:
                return SortBeanListingsRoaster(unsorted, method.IsLowToHigh);
        }
    }

    // Order by having countries of origin, and no regions - then fair trade, direct trade, organic certification
    private IEnumerable<BeanListingModel> SortBeanListingsDefault(IEnumerable<BeanListingModel> unsorted)
    {
        return unsorted.OrderByDescending(l => l.Bean.RoastLevel != RoastLevel.Green)
            .ThenByDescending(l => l.Bean.GetTraceabilityScore())
            .ThenByDescending(l => l.Bean.IsAboveFairTradePricing)
            .ThenByDescending(l => l.Bean.IsFairTradeCertified)
            .ThenByDescending(l => l.Bean.IsDirectTradeCertified)
            .ThenByDescending(l => l.Bean.OrganicCerification == OrganicCertification.Certified_Organic)
            .ThenByDescending(l => l.Bean.OrganicCerification == OrganicCertification.Uncertified_Organic).ToList();
    }

    private IEnumerable<BeanListingModel> SortBeanListingsPrice(IEnumerable<BeanListingModel> unsorted,
        bool IsLowToHigh)
    {
        return unsorted.OrderBy(l => IsLowToHigh ? l.Bean.GetPricePerOz() : -1M * l.Bean.GetPricePerOz())
            .ThenByDescending(l => l.Bean.RoastLevel != RoastLevel.Green)
            .ThenByDescending(l => l.Bean.GetTraceabilityScore())
            .ThenByDescending(l => l.Bean.IsAboveFairTradePricing)
            .ThenByDescending(l => l.Bean.IsFairTradeCertified)
            .ThenByDescending(l => l.Bean.IsDirectTradeCertified)
            .ThenByDescending(l => l.Bean.OrganicCerification == OrganicCertification.Certified_Organic)
            .ThenByDescending(l => l.Bean.OrganicCerification == OrganicCertification.Uncertified_Organic).ToList();
    }

    private IEnumerable<BeanListingModel> SortBeanListingsDateAdded(IEnumerable<BeanListingModel> unsorted,
        bool IsLowToHigh)
    {
        // In this case isLowToHigh means is oldest to newest
        // true = oldest
        return unsorted.OrderBy(l =>
                IsLowToHigh
                    ? (l.Bean.DateAdded - DateTime.MinValue).TotalDays
                    : (DateTime.MinValue - l.Bean.DateAdded).TotalDays)
            .ThenByDescending(l => l.Bean.RoastLevel != RoastLevel.Green)
            .ThenByDescending(l => l.Bean.GetTraceabilityScore())
            .ThenByDescending(l => l.Bean.IsAboveFairTradePricing)
            .ThenByDescending(l => l.Bean.IsFairTradeCertified)
            .ThenByDescending(l => l.Bean.IsDirectTradeCertified)
            .ThenByDescending(l => l.Bean.OrganicCerification == OrganicCertification.Certified_Organic)
            .ThenByDescending(l => l.Bean.OrganicCerification == OrganicCertification.Uncertified_Organic).ToList();
    }

    private IEnumerable<BeanListingModel> SortBeanListingsAlphabetical(IEnumerable<BeanListingModel> unsorted,
        bool IsLowToHigh)
    {
        // No need to sort by other fields because bean names are relatively unique
        if (IsLowToHigh)
        {
            return unsorted.OrderBy(l => l.Bean.FullName).ToList();
        }

        return unsorted.OrderByDescending(l => l.Bean.FullName).ToList();
    }

    private IEnumerable<BeanListingModel> SortBeanListingsRoaster(IEnumerable<BeanListingModel> unsorted,
        bool IsLowToHigh)
    {
        if (IsLowToHigh)
        {
            return unsorted.OrderBy(l => l.Roaster.Name)
                .ThenByDescending(l => l.Bean.RoastLevel != RoastLevel.Green)
                .ThenByDescending(l => l.Bean.GetTraceabilityScore())
                .ThenByDescending(l => l.Bean.IsAboveFairTradePricing)
                .ThenByDescending(l => l.Bean.IsFairTradeCertified)
                .ThenByDescending(l => l.Bean.IsDirectTradeCertified)
                .ThenByDescending(l => l.Bean.OrganicCerification == OrganicCertification.Certified_Organic)
                .ThenByDescending(l => l.Bean.OrganicCerification == OrganicCertification.Uncertified_Organic)
                .ToList();
        }

        return unsorted.OrderByDescending(l => l.Roaster.Name)
            .ThenByDescending(l => l.Bean.RoastLevel != RoastLevel.Green)
            .ThenByDescending(l => l.Bean.GetTraceabilityScore())
            .ThenByDescending(l => l.Bean.IsAboveFairTradePricing)
            .ThenByDescending(l => l.Bean.IsFairTradeCertified)
            .ThenByDescending(l => l.Bean.IsDirectTradeCertified)
            .ThenByDescending(l => l.Bean.OrganicCerification == OrganicCertification.Certified_Organic)
            .ThenByDescending(l => l.Bean.OrganicCerification == OrganicCertification.Uncertified_Organic).ToList();
    }
}