using HtmlAgilityPack;
using RoasterBeansDataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoasterBeansDataAccess.Parsers
{
	public class CloudCityParser
	{
		private static List<string> excludedTerms = new List<string> { "subscription", "sampler" };
		private const string baseUrl = "https://www.cloudcitycoffeeroasting.com";

		public async static Task<List<BeanModel>> ParseBeans(RoasterModel roaster)
		{
			// Get single-origin page first
			List<BeanModel> beanResults = await ParsePage(roaster.ShopURL, roaster, true);
			// Get blends next
			beanResults.AddRange(await ParsePage("https://www.cloudcitycoffeeroasting.com/collections/blends", roaster, false));

			return beanResults;
		}

		private async static Task<List<BeanModel>> ParsePage(string pageURL, RoasterModel roaster, bool isSingleOrigin)
		{
			string? content = await BeanDataScraper.GetPageContent(pageURL);
			if (String.IsNullOrEmpty(content))
			{
				return new List<BeanModel>();
			}

			HtmlDocument htmlDoc = new HtmlDocument();
			htmlDoc.LoadHtml(content);

			HtmlNode shopParent = htmlDoc.DocumentNode.SelectSingleNode("//div[contains(@class, 'collection-page__product-list')]");
			List<HtmlNode> shopItems = shopParent.SelectNodes(".//article").ToList();

			List<BeanModel> listings = new List<BeanModel>();

			foreach (HtmlNode productListing in shopItems)
			{
				BeanModel listing = new BeanModel();

				string imageURL = "https:" + productListing.SelectSingleNode(".//img").GetAttributeValue("src", "");
				string productURL = baseUrl + productListing.SelectSingleNode(".//a[contains(@class, 'product-item__image-link')]").GetAttributeValue("href", "");

				listing.ProductURL = productURL;
				listing.ImageURL = imageURL;

				string name = productListing.SelectSingleNode(".//h3").SelectSingleNode(".//a").InnerText.Trim();
				listing.FullName = name;

				string price = productListing.SelectSingleNode(".//span[contains(@class, 'money')]").InnerText.Replace("$", "").Trim();
				
				decimal parsedPrice;
				if (Decimal.TryParse(price, out parsedPrice))
				{
					listing.PriceBeforeShipping = parsedPrice;
				}

				listing.AvailablePreground = true;
				listing.SizeOunces = 12;

				listing.SetOriginsFromName();
				listing.IsSingleOrigin = isSingleOrigin;

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
