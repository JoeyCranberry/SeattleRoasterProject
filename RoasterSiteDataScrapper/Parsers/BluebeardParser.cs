using HtmlAgilityPack;
using RoasterBeansDataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoasterBeansDataAccess.Parsers
{
	public class BluebeardParser
	{
		private static List<string> excludedTerms = new List<string> { "instant", "subscription", "gift" };
		private const string baseURL = "https://bluebeardcoffee.com";

		public static List<BeanModel> ParseBeans(HtmlDocument shopHTML, RoasterModel roaster)
		{
			HtmlNode shopParent = shopHTML.DocumentNode.SelectSingleNode("//div[contains(@class, 'mainCollectionProductGrid')]");
			List<HtmlNode> shopItems = shopParent.SelectNodes(".//a").ToList();

			List<BeanModel> listings = new List<BeanModel>();

			foreach (HtmlNode productListing in shopItems)
			{
				BeanModel listing = new BeanModel();

				string imageURL = "https:" + productListing.SelectSingleNode(".//img").GetAttributeValue("src", "");
				string productURL = baseURL + productListing.GetAttributeValue("href", "");

				listing.ProductURL = productURL;
				listing.ImageURL = imageURL;

				string name = productListing.SelectSingleNode(".//h3").ChildNodes[1].InnerText.Trim();
				listing.FullName = name;

				HtmlNode priceNode = productListing.SelectSingleNode(".//span[contains(@class, 'price-regular')]");

				if(priceNode != null)
				{
					string? price = priceNode.InnerText.Replace("From $", "").Trim();

					if (!String.IsNullOrEmpty(price))
					{
						decimal parsedPrice;
						if (Decimal.TryParse(price, out parsedPrice))
						{
							listing.PriceBeforeShipping = parsedPrice;
						}
					}
				}
				
				listing.AvailablePreground = true;

				listing.SetOriginsFromName();
				listing.SetDecafFromName();

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
