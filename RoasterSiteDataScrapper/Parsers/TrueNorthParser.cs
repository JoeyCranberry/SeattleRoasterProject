using HtmlAgilityPack;
using RoasterBeansDataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoasterBeansDataAccess.Parsers
{
	internal class TrueNorthParser
	{
		private const string baseURL = "https://www.drinktruenorth.com";

		public static List<BeanModel> ParseBeans(HtmlDocument shopHTML, RoasterModel roaster)
		{
			HtmlNode shopParent = shopHTML.DocumentNode.SelectSingleNode(".//div[contains(@class, 'collection-grid')]");
			List<HtmlNode> shopItems = shopParent.SelectNodes(".//div[contains(@class, 'grid-item')]").ToList();

			// Remove elements with only one child because the <hr> tag is a grid-item
			shopItems.RemoveAll(i => i.ChildNodes.Count == 1);

			List<BeanModel> listings = new List<BeanModel>();

			foreach (HtmlNode productListing in shopItems)
			{
				BeanModel listing = new BeanModel();

				string imageURL = "https:" + productListing.SelectSingleNode(".//img").GetAttributeValue("src", "");

				string productURL = baseURL + productListing.SelectSingleNode(".//a").GetAttributeValue("href", "");

				listing.ImageURL = imageURL;
				listing.ProductURL = productURL;

				string name = productListing.SelectSingleNode(".//a[contains(@class, 'product-link')]").InnerText.Trim();
				listing.FullName = name;

				string price = productListing.SelectSingleNode(".//p").SelectSingleNode("./span").InnerText.Replace("$", "").Trim();

				decimal parsedPrice;
				if (Decimal.TryParse(price, out parsedPrice))
				{
					listing.PriceBeforeShipping = parsedPrice;
				}

				listing.AvailablePreground = false;
				listing.SizeOunces = 12;

				listing.SetRoastLevelFromName();
				listing.SetDecafFromName();
				listing.SetOriginsFromName();
				listing.SetProcessFromName();

				listing.MongoRoasterId = roaster.Id;
				listing.RoasterId = roaster.RoasterId;
				listing.DateAdded = DateTime.Now;

				listings.Add(listing);
			}

			return listings;
		}
	}
}
