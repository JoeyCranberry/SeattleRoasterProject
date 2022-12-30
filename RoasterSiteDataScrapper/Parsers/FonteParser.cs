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
	public class FonteParser
	{
		private const string blendsPageURL = "https://www.fontecoffee.com/coffee/blends/";

		public async static Task<ParseContentResult> ParseBeansForRoaster(RoasterModel roaster)
		{
			ParseContentResult overallResult = new()
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
				HtmlDocument htmlDoc = new();
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
			ParseContentResult result = new();

			HtmlNode shopParent = shopHTML.DocumentNode.SelectSingleNode("//div[@class='product-collection-item-list']");
			if (shopParent == null)
			{
				result.IsSuccessful = false;
				return result;
			}

			List<HtmlNode>? shopItems = shopParent.SelectNodes(".//figure")?.ToList();
			if (shopItems == null)
			{
				result.IsSuccessful = false;
				return result;
			}

			List<BeanModel> listings = new();

			foreach (HtmlNode productListing in shopItems)
			{
				BeanModel listing = new();

				try
				{
					string imageURL = productListing.SelectSingleNode(".//img").GetAttributeValue("src", "");
					string productURL = productListing.SelectSingleNode(".//a[contains(@class, 'product-item-image-link')]").GetAttributeValue("href", "");

					listing.ProductURL = productURL;
					listing.ImageURL = imageURL;

					string name = productListing.SelectSingleNode(".//h5[@class='product-item-name']")
						.SelectSingleNode("./a")
						.InnerText.Trim();
					listing.FullName = name;

					string price = productListing.SelectSingleNode(".//span[contains(@class, 'product-price')]").InnerText.Replace("$", "").Trim();

					if (Decimal.TryParse(price, out decimal parsedPrice))
					{
						listing.PriceBeforeShipping = parsedPrice;
					}

					listing.AvailablePreground = true;
					listing.SetOriginsFromName();
					listing.IsSingleOrigin = isSingleOrigin;
					listing.SetProcessFromName();
					listing.SetDecafFromName();
					listing.SetOrganicFromName();

					// "White Label" beans have size in name
					if (name.Contains("[8oz]"))
					{
						name = name.Replace("[8oz]", "");
						listing.SizeOunces = 8;
					}
					else
					{
						listing.SizeOunces = 12;
					}

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

			result.IsSuccessful = true;
			result.Listings = listings;

			return result;
		}
	}
}
