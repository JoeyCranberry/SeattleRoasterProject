using HtmlAgilityPack;
using RoasterBeansDataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoasterBeansDataAccess.Parsers
{
	internal class HaitiParser
	{
		private const string baseURL = "https://kay-tita-ti-mache.com";

		public static List<BeanModel> ParseBeans(HtmlDocument shopHTML, RoasterModel roaster)
		{
			HtmlNode shopParent = shopHTML.DocumentNode.SelectSingleNode("//div[contains(@class, 'border-left-solid-light')]");
			List<HtmlNode> shopItems = shopParent.SelectNodes("./div").ToList();

			List<BeanModel> listings = new List<BeanModel>();

			foreach (HtmlNode productListing in shopItems)
			{
				BeanModel listing = new BeanModel();

				string imageURL = "https:" + productListing.SelectSingleNode(".//img").GetAttributeValue("src", "");
				string productURL = baseURL + productListing.SelectSingleNode(".//a").GetAttributeValue("href", "");

				listing.ImageURL = imageURL;
				listing.ProductURL = productURL;

				string name = productListing.SelectSingleNode(".//div[contains(@class, 'card-body')]").SelectSingleNode(".//p").InnerText.Trim();
				listing.FullName = name;

				string price = productListing.SelectSingleNode(".//span[contains(@class, 'product-price__price')]").InnerText.Replace("$", "").Trim();

				decimal parsedPrice;
				if (Decimal.TryParse(price, out parsedPrice))
				{
					listing.PriceBeforeShipping = parsedPrice;
				}

				listing.AvailablePreground = false;
				listing.SizeOunces = 12;

				listing.SetRoastLevelFromName();

				listing.MongoRoasterId = roaster.Id;
				listing.RoasterId = roaster.RoasterId;
				listing.DateAdded = DateTime.Now;

				listings.Add(listing);
			}

			return listings;
		}
	}
}
