using HtmlAgilityPack;
using RoasterBeansDataAccess.DataAccess;
using RoasterBeansDataAccess.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoasterBeansDataAccess.Parsers
{
	public class CaffeUmbriaParser
	{
		private const string baseURL = "https://caffeumbria.com";
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

			List<HtmlNode>? shopItems = shopParent.SelectNodes(".//div[@class = 'product col col--1of3']")?.ToList();
			if (shopItems == null)
			{
				result.IsSuccessful = false;
				return result;
			}

			HtmlNode? featuredNode = shopHTML.DocumentNode.SelectSingleNode("//div[contains(@class, 'products--featured')]")?.SelectSingleNode("./div");
			if (featuredNode != null) 
			{
				shopItems.Add(featuredNode);
			}
			
			List<BeanModel> listings = new List<BeanModel>();

			TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
			foreach (HtmlNode productListing in shopItems)
			{
				BeanModel listing = new BeanModel();

				try
				{
					string imageURL = productListing.SelectSingleNode(".//div[@class='product__image']")
					.GetAttributeValue("style", "")
					.Replace("background-image: url(", "")
					.Replace(");", "");
					imageURL = "https:" + imageURL;

					string productURL = baseURL + productListing.SelectSingleNode(".//a").GetAttributeValue("href", "");

					listing.ProductURL = productURL;
					listing.ImageURL = imageURL;

					HtmlNode titleNode = productListing.SelectSingleNode(".//h2[contains(@class, 'product__title')]");
					HtmlNode titleSpanNode = titleNode.SelectSingleNode(".//span");
					string name;
					if (titleSpanNode != null)
					{
						name = titleSpanNode.InnerText;
					}
					else
					{
						name = titleNode.InnerText;
					}

					name = textInfo.ToTitleCase(name.ToLower());
					listing.FullName = name;

					listing.AvailablePreground = true;

					string? addlInfo = productListing.SelectSingleNode(".//div[contains(@class, 'product__info-items')]")?.InnerText.ToLower();
					if (addlInfo != null)
					{
						if (addlInfo.Contains("blend"))
						{
							listing.IsSingleOrigin = false;
						}

						if (addlInfo.Contains(value: "light"))
						{
							listing.RoastLevel = RoastLevel.LIGHT;
						}
						else if (addlInfo.Contains(value: "medium"))
						{
							listing.RoastLevel = RoastLevel.MEDIUM;
						}
						else if (addlInfo.Contains(value: "dark"))
						{
							listing.RoastLevel = RoastLevel.DARK;
						}

						if (addlInfo.Contains("decaf"))
						{
							listing.IsDecaf = true;
						}
					}

					listing.SizeOunces = 12M;

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
