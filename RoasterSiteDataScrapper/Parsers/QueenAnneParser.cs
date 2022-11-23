using HtmlAgilityPack;
using RoasterBeansDataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoasterBeansDataAccess.Parsers
{
	internal class QueenAnneParser
	{
		private static List<string> excludedTerms = new List<string> { "package", "gift", "pdf" };
		private const string baseURL = "https://queenanneroasters.com";

		public static List<BeanModel> ParseBeans(HtmlDocument shopHTML, RoasterModel roaster)
		{
			HtmlNode shopParent = shopHTML.DocumentNode.SelectSingleNode(".//div[contains(@class, 'ProductList-grid')]");
			List<HtmlNode> shopItems = shopParent.SelectNodes("./div[contains(@class, 'ProductList-item')]").ToList();

			List<BeanModel> listings = new List<BeanModel>();

			foreach (HtmlNode productListing in shopItems)
			{
				BeanModel listing = new BeanModel();

				string imageURL = productListing.SelectSingleNode(".//img").GetAttributeValue("src", "");

				string productURL = baseURL + productListing.SelectSingleNode(".//a").GetAttributeValue("href", "");

				listing.ImageURL = imageURL;
				listing.ProductURL = productURL;

				string name = productListing.SelectSingleNode(".//h1[contains(@class, 'ProductList-title')]").InnerText.Trim();
				listing.FullName = name;

				string price = productListing.SelectSingleNode(".//div[contains(@class, 'product-price')]").InnerText.Replace("$", "").Trim();

				decimal parsedPrice;
				if (Decimal.TryParse(price, out parsedPrice))
				{
					listing.PriceBeforeShipping = parsedPrice;
				}

				if(name.ToLower().Contains("8 oz"))
				{
					listing.SizeOunces = 8;
				}
				else
				{
					listing.SizeOunces = 12;
				}

				listing.AvailablePreground = true;

				listing.SetRoastLevelFromName();
				listing.SetDecafFromName();
				listing.SetOriginsFromName();
				listing.SetProcessFromName();

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
