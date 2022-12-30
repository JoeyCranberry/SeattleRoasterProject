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
	internal class ZokaParser
	{
		private static List<string> excludedTerms = new List<string> { "5lb", "gift", "beanies", "tee" };
		private const string baseURL = "https://www.zokacoffee.com";

		private const string page2URL = "https://www.zokacoffee.com/collections/coffee?page=2";

		public async static Task<ParseContentResult> ParseBeansForRoaster(RoasterModel roaster)
		{
			ParseContentResult overallResult = new ParseContentResult()
			{
				Listings = new List<BeanModel>(),
				IsSuccessful = false
			};

			overallResult = await ParsePage(overallResult, roaster.ShopURL, roaster);
			overallResult = await ParsePage(overallResult, page2URL, roaster);

			return overallResult;
		}


		private static async Task<ParseContentResult> ParsePage(ParseContentResult overallResult, string pageURL, RoasterModel roaster)
		{
			string? shopContent = await PageContentAccess.GetPageContent(pageURL);
			if (!String.IsNullOrEmpty(shopContent))
			{
				HtmlDocument htmlDoc = new HtmlDocument();
				htmlDoc.LoadHtml(shopContent);

				ParseContentResult parseResult = ParseBeans(htmlDoc, roaster);

				if (parseResult.IsSuccessful && parseResult.Listings != null && overallResult.Listings != null)
				{
					overallResult.Listings.AddRange(parseResult.Listings);
					overallResult.FailedParses += parseResult.FailedParses;
					overallResult.IsSuccessful = true;
				}
			}

			return overallResult;
		}

		private static ParseContentResult ParseBeans(HtmlDocument shopHTML, RoasterModel roaster)
		{
			ParseContentResult result = new ParseContentResult();

			HtmlNode shopParent = shopHTML.DocumentNode.SelectSingleNode("//ul[@id='product-grid']");
			if (shopParent == null)
			{
				result.IsSuccessful = false;
				return result;
			}

			List<HtmlNode>? shopItems = shopParent.SelectNodes("./li")?.ToList();
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
					string imageURL = "https:" + productListing.SelectSingleNode(".//img").GetAttributeValue("src", "");
					string productURL = baseURL + productListing.SelectSingleNode(".//a").GetAttributeValue("href", "");

					listing.ProductURL = productURL;
					listing.ImageURL = imageURL;

					string name = productListing.SelectSingleNode(".//a").InnerText.Replace("&amp;", "&").Trim();
					listing.FullName = name;

					string price = productListing.SelectSingleNode(".//span[contains(@class, 'price-item--regular')]").InnerText.Replace("$", "").Trim();

					decimal parsedPrice;
					if (Decimal.TryParse(price, out parsedPrice))
					{
						listing.PriceBeforeShipping = parsedPrice;
					}

					listing.SizeOunces = 12;

					listing.AvailablePreground = true;
					listing.SetOriginsFromName();

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
