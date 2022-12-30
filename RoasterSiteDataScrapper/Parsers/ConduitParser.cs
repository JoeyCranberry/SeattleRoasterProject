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
	public class ConduitParser
	{
		private static List<string> excludedTerms = new List<string> { "pouches" };
		private const string baseURL = "https://www.conduitcoffee.com";
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

			HtmlNode shopParent = shopHTML.DocumentNode.SelectSingleNode("//div[contains(@class, 'products')]");
			if (shopParent == null)
			{
				result.IsSuccessful = false;
				return result;
			}

			List<HtmlNode>? shopItems = shopParent.SelectNodes("./div[contains(@class, 'product')]")?.ToList();
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

					string name = productListing.SelectSingleNode(".//h4").InnerText.Trim();
					listing.FullName = name;

					string price;
					HtmlNode priceSiblingNode = productListing.SelectSingleNode(".//small");
					if (priceSiblingNode != null)
					{
						price = priceSiblingNode.NextSibling.InnerHtml;
					}
					else
					{
						price = productListing.SelectSingleNode(".//span[contains(@class, 'price')]").InnerText;
					}
					price = price.Replace("$", "").Trim();

					decimal parsedPrice;
					if (Decimal.TryParse(price, out parsedPrice))
					{
						listing.PriceBeforeShipping = parsedPrice;
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
