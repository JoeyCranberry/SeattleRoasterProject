using HtmlAgilityPack;
using RoasterBeansDataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoasterBeansDataAccess.Parsers
{
	internal class VictrolaParser
	{
		private static List<string> excludedTerms = new List<string> { "steeped", "subscription" };
		private const string baseURL = "https://www.victrolacoffee.com";

		public async static Task<List<BeanModel>> ParseBeans(RoasterModel roaster)
		{
			// Page 1
			List<BeanModel> beanResults = await ParsePage(roaster.ShopURL, roaster);
			// Page 2
			beanResults.AddRange(await ParsePage("https://www.victrolacoffee.com/collections/all-coffee-offerings?page=2", roaster));

			return beanResults;
		}

		private async static Task<List<BeanModel>> ParsePage(string pageURL, RoasterModel roaster)
		{
			string? content = await BeanDataScraper.GetPageContent(pageURL);
			if (String.IsNullOrEmpty(content))
			{
				return new List<BeanModel>();
			}

			HtmlDocument htmlDoc = new HtmlDocument();
			htmlDoc.LoadHtml(content);

			HtmlNode shopParent = htmlDoc.DocumentNode.SelectSingleNode("//div[contains(@class, 'collection-products')]");
			List<HtmlNode>? shopItems = shopParent.SelectNodes("./div[@class='product-list-item']")?.ToList();

			List<BeanModel> listings = new List<BeanModel>();

			if (shopItems == null )
			{
				return listings;
			}

			foreach (HtmlNode productListing in shopItems)
			{
				BeanModel listing = new BeanModel();

				string imageURL = "https:" + productListing.SelectSingleNode(".//img").GetAttributeValue("src", "");
				string productURL = baseURL + productListing.SelectSingleNode(".//a").GetAttributeValue("href", "");

				listing.ProductURL = productURL;
				listing.ImageURL = imageURL;

				string name = productListing.SelectSingleNode(".//h3").InnerText.Trim();
				listing.FullName = name;

				string price = productListing.SelectSingleNode(".//p[@class='product-list-item-price']").InnerText.Replace("$ ", "").Trim();

				decimal parsedPrice;
				if (Decimal.TryParse(price, out parsedPrice))
				{
					listing.PriceBeforeShipping = parsedPrice;
				}

				listing.SizeOunces = 12;
				listing.IsAboveFairTradePricing = true;

				listing.AvailablePreground = false;
				listing.SetOriginsFromName();

				listing.SetProcessFromName();
				listing.SetDecafFromName();
				listing.SetOrganicFromName();

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
