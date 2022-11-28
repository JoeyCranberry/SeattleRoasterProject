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
	public class BroadcastParser
	{
		private const string baseURL = "https://broadcastcoffeeroasters.com";

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

			HtmlNode shopParent = shopHTML.DocumentNode.SelectSingleNode("//ul[contains(@class, 'product-grid')]");
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
				BeanModel listing = new BeanModel();

				try
				{
					string imageURL = "https:" + productListing.SelectSingleNode(".//img").GetAttributeValue("src", "");
					HtmlNode titleLinkNode = productListing.SelectSingleNode(".//a[contains(@class, 'full-unstyled-link')]");
					string productURL = baseURL + titleLinkNode.GetAttributeValue("href", "");

					listing.ProductURL = productURL;
					listing.ImageURL = imageURL;

					string name = titleLinkNode.InnerText.Trim();
					listing.FullName = name;
					// card-information
					HtmlNode priceAndInfoNode = productListing.SelectSingleNode("//div[contains(@class, 'card-information')]");
					HtmlNode regularPrice = priceAndInfoNode.SelectSingleNode(".//span[contains(@class, 'price-item--regular')]");
					string price = "0";
					if (regularPrice != null)
					{
						price = regularPrice.InnerText.Replace("From $", "").Trim();
					}
					// If item is on sale - reg price is place in a <s> elem instead of <span>
					else
					{
						priceAndInfoNode.SelectSingleNode(".//s[contains(@class, 'price-item--regular')]").InnerText
							.Replace("$", "")
							.Replace("USD", "")
							.Trim();
					}

					decimal parsedPrice;
					if (Decimal.TryParse(price, out parsedPrice))
					{
						listing.PriceBeforeShipping = parsedPrice;
					}

					listing.AvailablePreground = true;
					listing.SizeOunces = 12;
					listing.SetOriginsFromName();
					listing.SetDecafFromName();

					// Listings have sub captions that can say, Blend, Single-Origin, or decaf
					string addlInfo = priceAndInfoNode.SelectSingleNode(".//div[contains(@class, 'caption-with-letter-spacing')]").InnerText.ToLower().Trim();
					switch (addlInfo)
					{
						case "Decaf":
							listing.IsDecaf = true;
							break;
						case "Single-Origin":
							listing.IsSingleOrigin = true;
							break;
						case "Blend":
							listing.IsSingleOrigin = false;
							break;
					}


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

			result.IsSuccessful = true;
			result.Listings = listings;

			return result;
		}
	}
}
