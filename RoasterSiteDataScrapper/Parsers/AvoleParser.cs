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
	internal class AvoleParser
	{
		private static List<string> excludedTerms = new List<string> { "jacket" };

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

			HtmlNode? shopParent = shopHTML.DocumentNode?.SelectSingleNode("//ul[contains(@class, 'S4WbK_')]");
			if (shopParent == null)
			{
				result.IsSuccessful = false;
				return result;
			}

			List<HtmlNode>? shopItems = shopParent.SelectNodes(".//li")?.ToList();
			if (shopItems == null)
			{
				result.IsSuccessful = false;
				return result;
			}

			List<BeanModel> listings = new List<BeanModel>();

			foreach (HtmlNode productListing in shopItems)
			{
				try
				{
					BeanModel listing = new BeanModel();

					string imageURL = productListing.SelectSingleNode(".//img").GetAttributeValue("src", "");
					string productURL = productListing.SelectSingleNode(".//a").GetAttributeValue("href", "");

					listing.ProductURL = productURL;
					listing.ImageURL = imageURL;

					HtmlNode nameAndPriceNode = productListing.SelectSingleNode(".//div[contains(@data-hook, 'not-image-container')]");
					string name = nameAndPriceNode.SelectSingleNode(".//h3").InnerText.Trim();
					listing.FullName = name;

					// Price can be out of stock
					// product-item-price-to-pay
					HtmlNode priceInStockNode = nameAndPriceNode.SelectSingleNode(".//span[contains(@data-hook, 'product-item-price-to-pay')]");
					HtmlNode priceOutOfStockNode = nameAndPriceNode.SelectSingleNode(".//span[contains(@data-hook, 'product-item-out-of-stock')]");

					if (priceInStockNode != null)
					{
						string price = priceInStockNode.NextSibling.InnerText.Replace("$", "").Trim();

						decimal parsedPrice;
						if (Decimal.TryParse(price, out parsedPrice))
						{
							listing.PriceBeforeShipping = parsedPrice;
						}
					}
					else if (priceOutOfStockNode != null)
					{
						listing.InStock = false;
					}
					else
					{
						listing.InStock = false;
						listing.PriceBeforeShipping = 0;
					}

					listing.AvailablePreground = true;
					// Avole is Ethopian only
					listing.CountriesOfOrigin = new List<SourceCountry>() { SourceCountry.ETHIOPIA };
					listing.SetDecafFromName();

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
