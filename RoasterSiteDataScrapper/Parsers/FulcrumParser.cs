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
	public class FulcrumParser
	{
		private static List<string> excludedTerms = new List<string> { "egift", "cold brew", "coffee tin", "tumbler", "gift certificate", "package", "sample" };


		public async static Task<ParseContentResult> ParseBeansForRoaster(RoasterModel roaster)
		{
			ParseContentResult overallResult = new ParseContentResult()
			{
				Listings = new List<BeanModel>(),
				IsSuccessful = false
			};

			// Get Fulcrum
			overallResult = await ParsePage(overallResult, "https://fulcrumcoffee.com/product-category/coffees/fulcrum/", roaster, 1);
			// Get Silver Cup
			overallResult = await ParsePage(overallResult, "https://fulcrumcoffee.com/product-category/coffees/silver-cup/", roaster, 0);
			// Get Urban City
			overallResult = await ParsePage(overallResult, "https://fulcrumcoffee.com/product-category/coffees/urban-city/", roaster, 0);

			return overallResult;
		}

		private static async Task<ParseContentResult> ParsePage(ParseContentResult overallResult, string pageURL, RoasterModel roaster, int waitTimes)
		{
			string? shopContent = await PageContentAccess.GetPageContent(pageURL, waitTimes);
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

			HtmlNode shopParent = shopHTML.DocumentNode.SelectSingleNode("//div[@class='ae-post-list-wrapper']");
			if (shopParent == null)
			{
				result.IsSuccessful = false;
				return result;
			}

			List<HtmlNode>? shopItems = shopParent.SelectNodes("./article")?.ToList();
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
					string imageURL = productListing.SelectSingleNode(".//img[contains(@class, 'attachment-')]").GetAttributeValue("src", "");

					string productURL = productListing.SelectSingleNode(".//a").GetAttributeValue("href", "");

					listing.ImageURL = imageURL;
					listing.ProductURL = productURL;

					string name = productListing.SelectSingleNode(".//div[contains(@class, 'elementor-text-editor')]").InnerText.Trim();
					listing.FullName = name;

					HtmlNode? priceNode = productListing.SelectSingleNode(".//span[contains(@class, 'woocommerce-Price-amount')]")?.SelectSingleNode("./bdi");
					if (priceNode != null)
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
