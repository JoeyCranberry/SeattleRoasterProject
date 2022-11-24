using HtmlAgilityPack;
using RoasterBeansDataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoasterBeansDataAccess.Parsers
{
	internal class UglyMugParser
	{
		private const string baseURL = "https://uglymugseattle.com";

		public static List<BeanModel> ParseBeans(HtmlDocument shopHTML, RoasterModel roaster)
		{
			HtmlNode shopParent = shopHTML.DocumentNode.SelectSingleNode(".//div[contains(@class, 'ProductList-grid')]");
			List<HtmlNode> shopItems = shopParent.SelectNodes("./div[contains(@class, 'ProductList-item')]").ToList();

			List<BeanModel> listings = new List<BeanModel>();

			foreach (HtmlNode productListing in shopItems)
			{
				BeanModel listing = new BeanModel();

				string imageURL = productListing.SelectSingleNode(".//img").GetAttributeValue("src", "");

				string productURL = baseURL + productListing.SelectSingleNode(".//a[@class='ProductList-item-link']").GetAttributeValue("href", "");

				listing.ImageURL = imageURL;
				listing.ProductURL = productURL;

				string name = productListing.SelectSingleNode(".//h1").InnerText.Trim();
				listing.FullName = name;

				string price = productListing.SelectSingleNode(".//div[@class='product-price']").InnerText.Replace("$", "").Trim();

				decimal parsedPrice;
				if (Decimal.TryParse(price, out parsedPrice))
				{
					listing.PriceBeforeShipping = parsedPrice;
				}

				HtmlNode soldOutNode = productListing.SelectSingleNode(".//div[contains(@class, 'sold-out')]");
				if(soldOutNode != null) 
				{
					listing.InStock = false;
				}

				listing.AvailablePreground = true;
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
