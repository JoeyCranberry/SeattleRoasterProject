using HtmlAgilityPack;
using RoasterBeansDataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoasterBeansDataAccess.Parsers
{
	internal class VahallaParser
	{
		public static List<BeanModel> ParseBeans(HtmlDocument shopHTML, RoasterModel roaster)
		{
			HtmlNode shopParent = shopHTML.DocumentNode.SelectSingleNode(".//div[contains(@class, 'w-grid')]");
			List<HtmlNode> shopItems = shopParent.SelectNodes("./div[contains(@class, 'grid__item')]").ToList();

			List<BeanModel> listings = new List<BeanModel>();

			foreach (HtmlNode productListing in shopItems)
			{
				BeanModel listing = new BeanModel();

				string imageURL = productListing.SelectSingleNode(".//img").GetAttributeValue("src", "");

				string productURL = "https://www.valhallacoffee.com/home";

				listing.ImageURL = imageURL;
				listing.ProductURL = productURL;

				string name = productListing.SelectSingleNode(".//p[contains(@class, 'w-product-title')]").InnerText.Trim();
				listing.FullName = name;

				string price = productListing.SelectSingleNode(".//p[contains(@class, 'product-price__wrapper')]")
					.SelectSingleNode("./span").InnerText.Substring(0, 6).Replace("$", "").Trim();

				decimal parsedPrice;
				if (Decimal.TryParse(price, out parsedPrice))
				{
					listing.PriceBeforeShipping = parsedPrice;
				}


				listing.AvailablePreground = true;
				listing.SizeOunces = 16;

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
