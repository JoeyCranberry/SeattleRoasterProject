using HtmlAgilityPack;
using RoasterBeansDataAccess.DataAccess;
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

		private const string blendsPageURL = "https://www.cloudcitycoffeeroasting.com/collections/blends";

		public async static Task<ParseContentResult> ParseBeansForRoaster(RoasterModel roaster)
		{
			ParseContentResult overallResult = new ParseContentResult()
			{
				Listings = new List<BeanModel>(),
				IsSuccessful = false
			};

			overallResult = await ParsePage(overallResult, roaster.ShopURL, roaster, true);
			overallResult = await ParsePage(overallResult, blendsPageURL, roaster, false);

			return overallResult;
		}

		private static async Task<ParseContentResult> ParsePage(ParseContentResult overallResult, string pageURL, RoasterModel roaster, bool isSingleOrigin)
		{
			string? shopContent = await PageContentAccess.GetPageContent(pageURL);
			if (!String.IsNullOrEmpty(shopContent))
			{
				HtmlDocument htmlDoc = new HtmlDocument();
				htmlDoc.LoadHtml(shopContent);

				ParseContentResult parseResult = ParseBeans(htmlDoc, roaster, isSingleOrigin);

				if (parseResult.IsSuccessful && parseResult.Listings != null && overallResult.Listings != null)
				{
					overallResult.Listings.AddRange(parseResult.Listings);
					overallResult.FailedParses += parseResult.FailedParses;
					overallResult.IsSuccessful = true;
				}
			}

			return overallResult;
		}

		private static ParseContentResult ParseBeans(HtmlDocument shopHTML, RoasterModel roaster, bool isSingleOrigin)
		{
			ParseContentResult result = new ParseContentResult();

			HtmlNode shopParent = shopHTML.DocumentNode.SelectSingleNode("//div[contains(@class, 'collection-page__product-list')]");
			List<HtmlNode> shopItems = shopParent.SelectNodes(".//article").ToList();

			List<BeanModel> listings = new List<BeanModel>();

			foreach (HtmlNode productListing in shopItems)
			{
				BeanModel listing = new BeanModel();

				try
				{
					string imageURL = "https:" + productListing.SelectSingleNode(".//img").GetAttributeValue("src", "");
					string productURL = baseUrl + productListing.SelectSingleNode(".//a[contains(@class, 'product-item__image-link')]").GetAttributeValue("href", "");

					listing.ProductURL = productURL;
					listing.ImageURL = imageURL;

					string name = productListing.SelectSingleNode(".//h3").SelectSingleNode(".//a").InnerText.Replace("🌱*NEW*", "").Trim();
					name = name.Replace("🌱 ", "").Replace("*NEW*", "").Trim();
					
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
				catch (Exception ex)
				{
					result.FailedParses++;
					result.exceptions.Add(ex);
				}
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

			result.IsSuccessful = true;
			result.Listings = listings;

			return result;
		}
	}
}
