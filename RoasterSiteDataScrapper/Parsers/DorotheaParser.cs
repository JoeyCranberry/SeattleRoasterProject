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
	public class DorotheaParser
	{
		private const string baseURL = "https://dorotheacoffee.com/";

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

			HtmlNode shopParent = shopHTML.DocumentNode.SelectSingleNode("//div[@data-section-id='featured-collection']");
			if (shopParent == null)
			{
				result.IsSuccessful = false;
				return result;
			}

			List<HtmlNode>? shopItems = shopParent.SelectNodes(".//div[contains(@class, 'grid__item')]")?.ToList();
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
					string imageURL;
					HtmlNode imageNode = productListing.SelectSingleNode(".//img[contains(@class, 'product__img')]");
					if (imageNode != null)
					{
						imageURL = imageNode.GetAttributeValue("data-srcset", "");
						imageURL = imageURL.Substring(2, imageURL.Length - 2);
						int index = imageURL.IndexOf("//");
						if (index != -1)
						{
							imageURL = imageURL.Substring(0, index);
							imageURL = imageURL.Replace(" 150w,", "");
							imageURL = "https://" + imageURL;
							listing.ImageURL = imageURL;
						}
					}


					string productURL = baseURL + productListing.SelectSingleNode(".//a").GetAttributeValue("href", "");

					listing.ProductURL = productURL;


					string name = productListing.SelectSingleNode(".//p[@class='grid-link__title']").InnerText.Trim();
					listing.FullName = name;

					string price = productListing.SelectSingleNode(".//p[@class='grid-link__meta']").InnerText.Replace("From $", "").Trim();

					decimal parsedPrice;
					if (Decimal.TryParse(price, out parsedPrice))
					{
						listing.PriceBeforeShipping = parsedPrice;
					}

					HtmlNode soldOutNode = productListing.SelectSingleNode(".//span[contains(@class, 'badge--sold-out')]");
					if (soldOutNode != null)
					{
						listing.InStock = false;
					}

					listing.AvailablePreground = true;
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
				}
			}

			result.IsSuccessful = true;
			result.Listings = listings;

			return result;
		}
	}
}
