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
	public class CampfireParser
	{
		private static List<string> excludedTerms = new List<string> { "sampler", "k-cups", "s'mores" };
		private const string baseURL = "https://www.welovecampfire.com";

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

			HtmlNode shopParent = shopHTML.DocumentNode.SelectSingleNode("//div[@id='productList']");
			if (shopParent == null)
			{
				result.IsSuccessful = false;
				return result;
			}

			List<HtmlNode>? shopItems = shopParent.SelectNodes("./a")?.ToList();
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
					string productURL = baseURL + productListing.GetAttributeValue("href", "");

					listing.ProductURL = productURL;
					listing.ImageURL = imageURL;

					string name = productListing.SelectSingleNode(".//div[@class='product-title']").InnerText.Trim();
					listing.FullName = name;

					string price = productListing.SelectSingleNode(".//div[@class='product-price']").InnerHtml.Replace("$", "");

					decimal parsedPrice;
					if (Decimal.TryParse(price, out parsedPrice))
					{
						listing.PriceBeforeShipping = parsedPrice;
					}

					HtmlNode soldOutNote = productListing.SelectSingleNode(".//div[contains(@class, 'sold-out')]");

					if (soldOutNote != null)
					{
						listing.InStock = false;
					}

					listing.AvailablePreground = false;
					listing.SizeOunces = 12;
					listing.SetOriginsFromName();
					listing.SetDecafFromName();
					listing.SetProcessFromName();
					listing.SetOrganicFromName();
					listing.SetDecafFromName();

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
