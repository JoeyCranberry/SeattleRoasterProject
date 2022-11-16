using HtmlAgilityPack;
using RoasterBeansDataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoasterBeansDataAccess.Parsers
{
	public class BoonBoonaParser
	{
		private static List<string> excludedTerms = new List<string> { "subscription", "thermos" };
		private const string baseURL = "https://www.boonboonacoffee.com";

		public static List<BeanModel> ParseBeans(HtmlDocument shopHTML, RoasterModel roaster)
		{
			HtmlNode shopParent = shopHTML.DocumentNode.SelectSingleNode("//div[contains(@class, 'product-list')]");
			List<HtmlNode> shopItems = shopParent.SelectNodes(".//div[contains(@class, 'product-wrap')]").ToList();

			List<BeanModel> listings = new List<BeanModel>();

			foreach (HtmlNode productListing in shopItems)
			{
				BeanModel listing = new BeanModel();

				string imageURL = "https:" + productListing.SelectSingleNode(".//img").GetAttributeValue("src", "");
				string productURL = baseURL + productListing.SelectSingleNode(".//a[contains(@class, 'product-info__caption')]").GetAttributeValue("href", "");

				listing.ProductURL = productURL;
				listing.ImageURL = imageURL;

				string name = productListing.SelectSingleNode(".//span[contains(@class, 'title')]").InnerText.Replace("(NEW)", "").Trim();
				listing.FullName = name;

				string price = productListing.SelectSingleNode(".//span[contains(@class, 'money')]").InnerText.Replace("$", "");
				decimal parsedPrice;
				if (Decimal.TryParse(price, out parsedPrice))
				{
					listing.PriceBeforeShipping = parsedPrice;
				}

				listing.SetOriginsFromName();
				listing.SetDecafFromName();
				listing.SetRoastLevelFromName();

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
