using HtmlAgilityPack;
using RoasterBeansDataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoasterBeansDataAccess.Parsers
{
	public class FulcrumParser
	{
		private static List<string> excludedTerms = new List<string> { "egift", "cold brew", "coffee tin", "tumbler", "gift certificate", "package", "sample" };


		public async static Task<List<BeanModel>> ParseBeans(RoasterModel roaster)
		{
			// Get Fulcrum
			List<BeanModel> beanResults = await ParsePage("https://fulcrumcoffee.com/product-category/coffees/fulcrum/", roaster, 1);
			// Get Silver Cup
			beanResults.AddRange(await ParsePage("https://fulcrumcoffee.com/product-category/coffees/silver-cup/", roaster, 0));
			// Get Urban City
			beanResults.AddRange(await ParsePage("https://fulcrumcoffee.com/product-category/coffees/urban-city/", roaster, 0));

			return beanResults;
		}
		private async static Task<List<BeanModel>> ParsePage(string pageURL, RoasterModel roaster, int waiTimes)
		{
			string? content = await BeanDataScraper.GetPageContent(pageURL, 1);
			if (String.IsNullOrEmpty(content))
			{
				return new List<BeanModel>();
			}

			HtmlDocument htmlDoc = new HtmlDocument();
			htmlDoc.LoadHtml(content);

			HtmlNode shopParent = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='ae-post-list-wrapper']");
			List<HtmlNode> shopItems = shopParent.SelectNodes("./article").ToList();

			List<BeanModel> listings = new List<BeanModel>();

			foreach (HtmlNode productListing in shopItems)
			{
				BeanModel listing = new BeanModel();

				string imageURL = productListing.SelectSingleNode(".//img[contains(@class, 'attachment-')]").GetAttributeValue("src", "");

				string productURL = productListing.SelectSingleNode(".//a").GetAttributeValue("href", "");

				listing.ImageURL = imageURL;
				listing.ProductURL = productURL;

				string name = productListing.SelectSingleNode(".//div[contains(@class, 'elementor-text-editor')]").InnerText.Trim();
				listing.FullName = name;

				HtmlNode? priceNode = productListing.SelectSingleNode(".//span[contains(@class, 'woocommerce-Price-amount')]")?.SelectSingleNode("./bdi");
				if(priceNode != null)
				{
					string price = priceNode.InnerText.Replace("$", "").Trim();

					decimal parsedPrice;
					if (Decimal.TryParse(price, out parsedPrice))
					{
						listing.PriceBeforeShipping = parsedPrice;
					}
				}

				listing.AvailablePreground = false;
				listing.SizeOunces = 12;

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
