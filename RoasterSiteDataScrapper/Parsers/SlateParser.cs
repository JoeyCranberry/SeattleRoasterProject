using HtmlAgilityPack;
using RoasterBeansDataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoasterBeansDataAccess.Parsers
{
	internal class SlateParser
	{
		public static List<BeanModel> ParseBeans(HtmlDocument shopHTML, RoasterModel roaster)
		{
			HtmlNode shopParent = shopHTML.DocumentNode.SelectSingleNode(".//div[contains(@class, 'products')]");
			List<HtmlNode> shopItems = shopParent.SelectNodes(".//div[contains(@class, 'col-inner')]").ToList();

			List<BeanModel> listings = new List<BeanModel>();

			foreach (HtmlNode productListing in shopItems)
			{
				BeanModel listing = new BeanModel();

				string imageURL = productListing.SelectSingleNode(".//img").GetAttributeValue("src", "");

				string productURL = productListing.SelectSingleNode(".//a").GetAttributeValue("href", "");

				listing.ImageURL = imageURL;
				listing.ProductURL = productURL;

				string name = productListing.SelectSingleNode(".//a[contains(@class, 'woocommerce-LoopProduct-link')]").InnerText.Trim();
				listing.FullName = name;

				string price = productListing.SelectSingleNode(".//bdi").InnerText.Replace("$", "").Trim();

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
