using HtmlAgilityPack;
using RoasterBeansDataAccess.DataAccess;
using RoasterBeansDataAccess.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoasterBeansDataAccess.Parsers
{
	internal class VinylParser
	{
		private static List<string> excludedTerms = new List<string> { "mug", "gift", "sticked", "t-shirt", "sticker", "subscription" };
		private const string baseURL = "https://www.vinylcoffeeroasters.com";

		public async static Task<ParseContentResult> ParseBeansForRoaster(RoasterModel roaster)
		{
			string? shopContent = await PageContentAccess.GetPageContent(roaster.ShopURL);
			if (!String.IsNullOrEmpty(shopContent))
			{
				HtmlDocument htmlDoc = new HtmlDocument();
				htmlDoc.LoadHtml(shopContent);

				return ParseBeans(htmlDoc, roaster);
			}

			return new ParseContentResult()
			{
				IsSuccessful = false
			};
		}

		private static ParseContentResult ParseBeans(HtmlDocument shopHTML, RoasterModel roaster)
		{
			ParseContentResult result = new ParseContentResult();

			HtmlNode? shopParent = shopHTML.DocumentNode.SelectSingleNode(".//div[@class='products-flex-container']")?.ChildNodes[1];
			if (shopParent == null)
			{
				result.IsSuccessful = false;
				return result;
			}

			List<HtmlNode>? shopItems = shopParent.SelectNodes(".//div[contains(@class, 'grid-item')]")?.ToList();
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
					string imageURL = productListing.SelectSingleNode(".//img").GetAttributeValue("data-src", "");

					string productURL = baseURL + productListing.SelectSingleNode(".//a").GetAttributeValue("href", "");

					listing.ImageURL = imageURL;
					listing.ProductURL = productURL;

					string name = productListing.SelectSingleNode(".//div[contains(@class, 'grid-title')]").InnerText.Trim();
					TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
					name = textInfo.ToTitleCase(name.ToLower());

					listing.FullName = name;

					string price = productListing.SelectSingleNode(".//div[contains(@class, 'product-price')]").InnerText.Replace("from $", "").Trim();

					decimal parsedPrice;
					if (Decimal.TryParse(price, out parsedPrice))
					{
						listing.PriceBeforeShipping = parsedPrice;
					}

					listing.AvailablePreground = true;
					listing.SizeOunces = 12;

					listing.SetRoastLevelFromName();
					listing.SetDecafFromName();
					listing.SetOriginsFromName();
					listing.SetProcessFromName();

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
