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
	public class BellinghamParser
	{
		private static List<string> excludedTerms = new List<string> { "mug", "thermos" };

		private const string blendsPageURL = "https://bellinghamcoffee.com/product-category/blends/";

		public async static Task<ParseContentResult> ParseBeansForRoaster(RoasterModel roaster)
		{
			ParseContentResult overallResult = new ParseContentResult() {
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

			HtmlNode? shopParent = shopHTML.DocumentNode?.SelectSingleNode("//ul[contains(@class, 'products')]");
			if (shopParent == null)
			{
				result.IsSuccessful = false;
				return result;
			}

			List<HtmlNode>? shopItems = shopParent?.SelectNodes(".//li")?.ToList();
			if (shopItems == null)
			{
				result.IsSuccessful = false;
				return result;
			}

			List<BeanModel> listings = new List<BeanModel>();

			foreach (HtmlNode productListing in shopItems)
			{
				BeanModel listing = new BeanModel();

				try
				{
					string imageURL = productListing.SelectSingleNode(".//img").GetAttributeValue("src", "");
					string productURL = productListing.SelectSingleNode(".//a").GetAttributeValue("href", "");

					listing.ProductURL = productURL;
					listing.ImageURL = imageURL;

					string name = productListing.SelectSingleNode(".//h2").FirstChild.InnerText.Trim();
					listing.FullName = name;

					HtmlNode soldOutNode = productListing.SelectSingleNode(".//div[contains(@class, 'rey-soldout-badge')]");
					if (soldOutNode != null)
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
				catch (Exception ex)
				{
					result.FailedParses++;
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
