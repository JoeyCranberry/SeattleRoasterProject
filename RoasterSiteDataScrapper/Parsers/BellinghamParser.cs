using HtmlAgilityPack;
using RoasterBeansDataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoasterBeansDataAccess.Parsers
{
	public class BellinghamParser
	{
		private static List<string> excludedTerms = new List<string> { "mug", "thermos" };

		public async static Task<List<BeanModel>> ParseBeans(RoasterModel roaster)
		{
			// Get single-origin page first
			List<BeanModel> beanResults = await ParsePage(roaster.ShopURL, roaster, true);
			// Get blends next
			beanResults.AddRange(await ParsePage("https://bellinghamcoffee.com/product-category/blends/", roaster, false));

			return beanResults;
		}

		private async static Task<List<BeanModel>> ParsePage(string pageURL, RoasterModel roaster, bool isSingleOrigin)
		{
			string? content = await BeanDataScraper.GetPageContent(pageURL);
			if(String.IsNullOrEmpty(content))
			{
				return new List<BeanModel>();
			}

			HtmlDocument htmlDoc = new HtmlDocument();
			htmlDoc.LoadHtml(content);

			HtmlNode shopParent = htmlDoc.DocumentNode.SelectSingleNode("//ul[contains(@class, 'products')]");
			List<HtmlNode> shopItems = shopParent.SelectNodes(".//li").ToList();

			List<BeanModel> listings = new List<BeanModel>();

			foreach (HtmlNode productListing in shopItems)
			{
				BeanModel listing = new BeanModel();

				string imageURL = productListing.SelectSingleNode(".//img").GetAttributeValue("src", "");
				string productURL = productListing.SelectSingleNode(".//a").GetAttributeValue("href", "");

				listing.ProductURL = productURL;
				listing.ImageURL = imageURL;

				string name = productListing.SelectSingleNode(".//h2").FirstChild.InnerText.Trim();
				listing.FullName = name;

				HtmlNode soldOutNode = productListing.SelectSingleNode(".//div[contains(@class, 'rey-soldout-badge')]");
				if(soldOutNode != null)
				{
					listing.InStock = false;
				}

				listing.AvailablePreground = false;
				listing.SetOriginsFromName();
				listing.IsSingleOrigin = isSingleOrigin;
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
