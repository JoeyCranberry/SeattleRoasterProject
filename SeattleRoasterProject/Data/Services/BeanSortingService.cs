using RoasterBeansDataAccess.Models;
using static SeattleRoasterProject.Data.Services.SortMethod;

namespace SeattleRoasterProject.Data.Services
{
	public class BeanSortingService
	{
		public List<BeanListingModel> SortBeanListings(List<BeanListingModel> unsorted, SortMethod method)
		{
			switch(method.SortByField)
			{
				default:
				case SORT_FIELD.DEFAULT:
					return SortBeanListingsDefault(unsorted);
				case SORT_FIELD.PRICE:
					return SortBeanListingsPrice(unsorted, method.IsLowToHigh);
				case SORT_FIELD.DATE_ADDED:
					return SortBeanListingsDateAdded(unsorted, method.IsLowToHigh);
				case SORT_FIELD.ALPHABETICAL:
					return SortBeanListingsAlphabetical(unsorted, method.IsLowToHigh);
				case SORT_FIELD.ROSATER:
					return SortBeanListingsRoaster(unsorted, method.IsLowToHigh);
			}
		}

		// Order by having countries of origin, and no regions - then fair trade, direct trade, organic certification
		private List<BeanListingModel> SortBeanListingsDefault(List<BeanListingModel> unsorted)
		{
			return unsorted.OrderByDescending(l => l.Bean.RoastLevel != RoastLevel.GREEN)
				.ThenByDescending(l => l.Bean.GetSourcingScore())
				.ThenByDescending(l => l.Bean.IsAboveFairTradePricing)
				.ThenByDescending(l => l.Bean.IsFairTradeCertified)
				.ThenByDescending(l => l.Bean.IsDirectTradeCertified)
				.ThenByDescending(l => l.Bean.OrganicCerification == OrganicCerification.CERTIFIED_ORGANIC)
				.ThenByDescending(l => l.Bean.OrganicCerification == OrganicCerification.UNCERTIFIED_ORGANIC).ToList();
		}

		private List<BeanListingModel> SortBeanListingsPrice(List<BeanListingModel> unsorted, bool IsLowToHigh)
		{
			return unsorted.OrderBy(l => IsLowToHigh ? l.Bean.GetPricePerOz() : (-1M)*l.Bean.GetPricePerOz())
				.ThenByDescending(l => l.Bean.RoastLevel != RoastLevel.GREEN)
				.ThenByDescending(l => l.Bean.GetSourcingScore())
				.ThenByDescending(l => l.Bean.IsAboveFairTradePricing)
				.ThenByDescending(l => l.Bean.IsFairTradeCertified)
				.ThenByDescending(l => l.Bean.IsDirectTradeCertified)
				.ThenByDescending(l => l.Bean.OrganicCerification == OrganicCerification.CERTIFIED_ORGANIC)
				.ThenByDescending(l => l.Bean.OrganicCerification == OrganicCerification.UNCERTIFIED_ORGANIC).ToList();
		}

		private List<BeanListingModel> SortBeanListingsDateAdded(List<BeanListingModel> unsorted, bool IsLowToHigh)
		{
			// In this case isLowToHigh means is oldest to newest
			return unsorted.OrderBy(l => IsLowToHigh ? l.Bean.DateAdded : DateTime.Now.AddDays( l.Bean.DateAdded.Day ) )
				.ThenByDescending(l => l.Bean.RoastLevel != RoastLevel.GREEN)
				.ThenByDescending(l => l.Bean.GetSourcingScore())
				.ThenByDescending(l => l.Bean.IsAboveFairTradePricing)
				.ThenByDescending(l => l.Bean.IsFairTradeCertified)
				.ThenByDescending(l => l.Bean.IsDirectTradeCertified)
				.ThenByDescending(l => l.Bean.OrganicCerification == OrganicCerification.CERTIFIED_ORGANIC)
				.ThenByDescending(l => l.Bean.OrganicCerification == OrganicCerification.UNCERTIFIED_ORGANIC).ToList();
		}

		private List<BeanListingModel> SortBeanListingsAlphabetical(List<BeanListingModel> unsorted, bool IsLowToHigh)
		{
			// No need to sort by other fields because bean names are relatively unique
			if(IsLowToHigh)
			{
				return unsorted.OrderBy(l => l.Bean.FullName).ToList();
			}
			else
			{
				return unsorted.OrderByDescending(l => l.Bean.FullName).ToList();
			}
		}

		private List<BeanListingModel> SortBeanListingsRoaster(List<BeanListingModel> unsorted, bool IsLowToHigh)
		{
			if (IsLowToHigh)
			{
				return unsorted.OrderBy(l => l.Roaster.Name)
					.ThenByDescending(l => l.Bean.RoastLevel != RoastLevel.GREEN)
					.ThenByDescending(l => l.Bean.GetSourcingScore())
					.ThenByDescending(l => l.Bean.IsAboveFairTradePricing)
					.ThenByDescending(l => l.Bean.IsFairTradeCertified)
					.ThenByDescending(l => l.Bean.IsDirectTradeCertified)
					.ThenByDescending(l => l.Bean.OrganicCerification == OrganicCerification.CERTIFIED_ORGANIC)
					.ThenByDescending(l => l.Bean.OrganicCerification == OrganicCerification.UNCERTIFIED_ORGANIC).ToList();
			}
			else
			{
				return unsorted.OrderByDescending(l => l.Roaster.Name)
					.ThenByDescending(l => l.Bean.RoastLevel != RoastLevel.GREEN)
					.ThenByDescending(l => l.Bean.GetSourcingScore())
					.ThenByDescending(l => l.Bean.IsAboveFairTradePricing)
					.ThenByDescending(l => l.Bean.IsFairTradeCertified)
					.ThenByDescending(l => l.Bean.IsDirectTradeCertified)
					.ThenByDescending(l => l.Bean.OrganicCerification == OrganicCerification.CERTIFIED_ORGANIC)
					.ThenByDescending(l => l.Bean.OrganicCerification == OrganicCerification.UNCERTIFIED_ORGANIC).ToList();
			}
		}
	}

	public class SortMethod
	{
		public bool IsLowToHigh { get; set; } = false;
		public SORT_FIELD SortByField { get; set; } = SORT_FIELD.DEFAULT;

		public enum SORT_FIELD
		{
			DEFAULT,
			PRICE,
			DATE_ADDED,
			ALPHABETICAL,
			ROSATER
		}
	}
}
