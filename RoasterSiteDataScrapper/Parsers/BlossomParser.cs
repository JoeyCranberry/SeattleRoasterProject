using HtmlAgilityPack;
using RoasterBeansDataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoasterBeansDataAccess.Parsers
{
	public class BlossomParser
	{
		private static List<string> excludedTerms = new List<string> { "subscription", "sampler" };
		private const string baseUrl = "https://blossomcoffeeroasters.com";

		public async static Task<List<BeanModel>> ParseBeans(RoasterModel roaster)
		{
			// Get single-origin page first
			List<BeanModel> beanResults = await ParsePage(roaster.ShopURL, roaster, true);
			// Get blends next
			beanResults.AddRange(await ParsePage("https://blossomcoffeeroasters.com/collections/blends", roaster, false));

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

			HtmlNode shopParent = htmlDoc.DocumentNode.SelectSingleNode("//div[@id ='CollectionAjaxContent']");
			List<HtmlNode> shopItems = shopParent.SelectNodes(".//div[contains(@class, 'grid-product__content')]").ToList(); 

			List<BeanModel> listings = new List<BeanModel>();

			foreach (HtmlNode productListing in shopItems)
			{
				BeanModel listing = new BeanModel();

				string imageURL = productListing.SelectSingleNode(".//div[contains(@class, 'grid__image-ratio')]")
					.GetAttributeValue("style", "")
					.Replace("180x", "360x")
					.Replace("background-image: url(&quot;", "")
					.Replace("&quot;);", "");
				string productURL = baseUrl + productListing.SelectSingleNode(".//a[contains(@class, 'grid-product__link ')]").GetAttributeValue("href", "");

				listing.ProductURL = productURL;
				listing.ImageURL = imageURL;

				string name = productListing.SelectSingleNode(".//div[contains(@class, 'grid-product__title')]").InnerText.Trim();
				listing.FullName = name;

				HtmlNode priceNode = productListing.SelectSingleNode(".//div[contains(@class, 'grid-product__price')]");
				string price = priceNode.SelectSingleNode(".//span").InnerText.Replace("from $", "").Trim();
				decimal parsedPrice;
				if (Decimal.TryParse(price, out parsedPrice))
				{
					listing.PriceBeforeShipping = parsedPrice;
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
