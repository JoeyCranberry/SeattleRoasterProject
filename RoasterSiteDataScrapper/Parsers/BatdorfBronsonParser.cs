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
	public class BatdorfBronsonParser
	{
		private const string baseURL = "https://www.dancinggoats.com";
		private static List<string> excludedTerms = new List<string> { "choice", "sample", "tumbler", "shirt", "tee", "glass", "beanie", "gift card", "anchorhead - coffee supply co", "4oz" };

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

			HtmlNode? shopParent = shopHTML.DocumentNode?.SelectSingleNode("//div[contains(@class, 'collection-products')]")?.ChildNodes[1];
			if (shopParent == null)
			{
				result.IsSuccessful = false;
				return result;
			}

			List<HtmlNode>? shopItems = shopParent?.SelectNodes(".//div[contains(@class, 'o-layout__item')]")?.ToList();
			if (shopItems == null)
			{
				result.IsSuccessful = false;
				return result;
			}

			shopItems.RemoveAll(child => child.ChildNodes.Count == 0);

			List<BeanModel> listings = new List<BeanModel>();

			foreach (HtmlNode productListing in shopItems)
			{
				BeanModel listing = new BeanModel();

				try
				{
					HtmlNode productLinkNode = productListing.SelectSingleNode(".//a[contains(@class, 'product-link')]");
					string productURL = baseURL + productLinkNode.GetAttributeValue("href", "");
					HtmlNode imgNode = productListing.SelectSingleNode(".//img");
					string imageURL = imgNode.GetAttributeValue("data-src", "");
					// Remove leading slashes
					if (!String.IsNullOrEmpty(imageURL))
					{
						imageURL = "https:" + imageURL.Replace("{width}", "180");
					}

					listing.ProductURL = productURL;
					listing.ImageURL = imageURL;

					string name = productLinkNode.GetAttributeValue("title", "");
					string price = productListing.SelectSingleNode(".//span[contains(@class, 'money')]").FirstChild.InnerText.Replace("$", "");

					decimal parsedPrice;
					if (Decimal.TryParse(price, out parsedPrice))
					{
						listing.PriceBeforeShipping = parsedPrice;
					}
					listing.FullName = name;

					listing.DateAdded = DateTime.Now;
					listing.RoasterId = roaster.RoasterId;
					listing.MongoRoasterId = roaster.Id;

					listing.SetOriginsFromName();
					listing.SetProcessFromName();
					listing.SetDecafFromName();
					listing.SetOrganicFromName();
					listing.SetFairTradeFromName();

					if(name.Contains("4oz"))
					{
						listing.SizeOunces = 4;
					}
					else
					{
						listing.SizeOunces = 12;
					}

					listing.AvailablePreground = true;

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
