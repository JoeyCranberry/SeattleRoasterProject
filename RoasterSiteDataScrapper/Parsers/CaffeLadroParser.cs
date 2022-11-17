using HtmlAgilityPack;
using RoasterBeansDataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoasterBeansDataAccess.Parsers
{
	public class CaffeLadroParser
	{
		private static List<string> excludedTerms = new List<string> { "subscription", "box", "cup", "steeped", "5lb" };

		public static List<BeanModel> ParseBeans(HtmlDocument shopHTML, RoasterModel roaster)
		{
			HtmlNode shopParent = shopHTML.DocumentNode.SelectSingleNode("//div[contains(@class, 'fluidContainer')]");
			List<HtmlNode> shopItems = shopParent.SelectNodes(".//div[contains(@class, 'tileContent')]").ToList();

			List<BeanModel> listings = new List<BeanModel>();

			foreach (HtmlNode productListing in shopItems)
			{
				BeanModel listing = new BeanModel();

				string productURL = productListing.SelectSingleNode(".//a[contains(@class, 'nextProdThumb')]").GetAttributeValue("href", "");
				string imageURL = "https:" + productListing.SelectSingleNode(".//img").GetAttributeValue("src", "");
				
				listing.ProductURL = productURL;
				listing.ImageURL = imageURL;

				string name = productListing.SelectSingleNode(".//a[contains(@class, 'nextProdName')]").InnerText.Replace("12oz", "");
				listing.FullName = name;

				string price = productListing.SelectSingleNode(".//div[contains(@class, 'nextPrice')]").SelectSingleNode(".//b").InnerText.Replace("$", "");

				if (!String.IsNullOrEmpty(price))
				{
					decimal parsedPrice;
					if (Decimal.TryParse(price, out parsedPrice))
					{
						listing.PriceBeforeShipping = parsedPrice;
					}
				}

				listing.AvailablePreground = true;

				listing.SetOriginsFromName();
				listing.SetDecafFromName();

				string? addlInfo = productListing.SelectSingleNode(".//div[contains(@class, 'nextShortDesc')]")?.InnerText.ToLower();
				if (!String.IsNullOrEmpty(addlInfo))
				{
					if (addlInfo.Contains("fair trade"))
					{
						listing.IsFairTradeCertified = true;
					}

					if (addlInfo.Contains("organic"))
					{
						listing.OrganicCerification = OrganicCerification.CERTIFIED_ORGANIC;
					}
				}

				listing.SizeOunces = 12M;

				listing.MongoRoasterId = roaster.Id;
				listing.RoasterId = roaster.RoasterId;
				listing.DateAdded = DateTime.Now;

				listings.Add(listing);
			}

			// Remove any excluded terms
			foreach (var product in listings)
			{
				foreach (string term in excludedTerms)
				{
					if (product.FullName.ToLower().Contains(term))
					{
						product.IsExcluded = true;
					}
				}
			}

			return listings;
		}
	}
}
