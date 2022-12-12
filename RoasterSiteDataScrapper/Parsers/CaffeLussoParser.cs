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
	public class CaffeLussoParser
	{
		private static List<string> excludedTerms = new List<string> { "gift", "sampler" };
		private const string baseURL = "https://caffelusso.com";

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

			HtmlNode? shopParent = shopHTML.DocumentNode.SelectSingleNode("//div[contains(@class, 'product-list')]");
			if (shopParent == null)
			{
				result.IsSuccessful = false;
				return result;
			}

			List<HtmlNode>? shopItems = shopParent.SelectNodes(".//div[contains(@class, 'one-third')]")?.ToList();
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

					HtmlNode productInfoNode = productListing.SelectSingleNode(".//a[contains(@class, 'product-info__caption')]");
					string productURL = baseURL + productInfoNode.GetAttributeValue("href", "");

					listing.ProductURL = productURL;
					listing.ImageURL = imageURL;

					string name = productInfoNode.SelectSingleNode(".//span[contains(@class, 'title')]").InnerText.Trim();
					listing.FullName = name;

					HtmlNode? priceNode = productListing.SelectSingleNode(".//span[contains(@class, 'money')]");
					if (priceNode != null)
					{
						string price = priceNode.InnerText.Replace("$", "");
						decimal parsedPrice;
						if (Decimal.TryParse(price, out parsedPrice))
						{
							listing.PriceBeforeShipping = parsedPrice;
						}
					}
					else
					{
						listing.InStock = false;
					}

					listing.SetOriginsFromName();
					listing.SetDecafFromName();
					listing.SetRoastLevelFromName();

					listing.SizeOunces = 12M;

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
