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
	internal class HerkimerParser
	{
		private const string singleOriginURL = "https://herkimercoffee.com/product-category/wholebean-coffee/single-origins/";
		private const string blendsPageURL = "https://herkimercoffee.com/product-category/wholebean-coffee/blends/";

		public async static Task<ParseContentResult> ParseBeansForRoaster(RoasterModel roaster)
		{
			ParseContentResult overallResult = new()
			{
				Listings = new List<BeanModel>(),
				IsSuccessful = false
			};

			overallResult = await ParsePage(overallResult, singleOriginURL, roaster, true);
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

			HtmlNode shopParent = shopHTML.DocumentNode.SelectSingleNode("//ul[contains(@class, 'products')]");
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

			List<BeanModel> listings = new();

			foreach (HtmlNode productListing in shopItems)
			{
				BeanModel listing = new();

				try
				{
					string imageURL = productListing.SelectSingleNode(".//img").GetAttributeValue("src", "");
					string productURL = productListing.SelectSingleNode(".//a").GetAttributeValue("href", "");

					listing.ProductURL = productURL;
					listing.ImageURL = imageURL;

					string name = productListing.SelectSingleNode(".//h2").InnerText.Trim();
					listing.FullName = name;

					string price = productListing.SelectSingleNode(".//bdi").InnerText.Replace("$", "").Trim();

					if (Decimal.TryParse(price, out decimal parsedPrice))
					{
						listing.PriceBeforeShipping = parsedPrice;
					}

					listing.SizeOunces = 12;

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
					result.exceptions.Add(ex);
				}
			}

			result.IsSuccessful = true;
			result.Listings = listings;

			return result;
		}
	}
}
