using HtmlAgilityPack;
using RoasterBeansDataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoasterBeansDataAccess.Parsers
{
	public class FonteParser
	{
		public async static Task<List<BeanModel>> ParseBeans(RoasterModel roaster)
		{
			// Get single-origin page first
			List<BeanModel> beanResults = await ParsePage("https://www.fontecoffee.com/coffee/single-origins/", roaster, true);
			// Get blends next
			beanResults.AddRange(await ParsePage("https://www.fontecoffee.com/coffee/blends/", roaster, false));

			return beanResults;
		}

		private async static Task<List<BeanModel>> ParsePage(string pageURL, RoasterModel roaster, bool isSingleOrigin)
		{
			string? content = await BeanDataScraper.GetPageContent(pageURL);
			if (String.IsNullOrEmpty(content))
			{
				return new List<BeanModel>();
			}

			HtmlDocument htmlDoc = new HtmlDocument();
			htmlDoc.LoadHtml(content);

			HtmlNode shopParent = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='product-collection-item-list']");
			List<HtmlNode> shopItems = shopParent.SelectNodes(".//figure").ToList();

			List<BeanModel> listings = new List<BeanModel>();

			foreach (HtmlNode productListing in shopItems)
			{
				BeanModel listing = new BeanModel();

				string imageURL = productListing.SelectSingleNode(".//img").GetAttributeValue("src", "");
				string productURL = productListing.SelectSingleNode(".//a[contains(@class, 'product-item-image-link')]").GetAttributeValue("href", "");

				listing.ProductURL = productURL;
				listing.ImageURL = imageURL;

				string name = productListing.SelectSingleNode(".//h5[@class='product-item-name']")
					.SelectSingleNode("./a")
					.InnerText.Trim();
				listing.FullName = name;

				string price = productListing.SelectSingleNode(".//span[contains(@class, 'product-price')]").InnerText.Replace("$", "").Trim();
				
				decimal parsedPrice;
				if (Decimal.TryParse(price, out parsedPrice))
				{
					listing.PriceBeforeShipping = parsedPrice;
				}

				listing.AvailablePreground = true;
				listing.SetOriginsFromName();
				listing.IsSingleOrigin = isSingleOrigin;
				listing.SetProcessFromName();
				listing.SetDecafFromName();
				listing.SetOrganicFromName();

				// "White Label" beans have size in name
				if(name.Contains("[8oz]"))
				{
					name.Replace("[8oz]", "");
					listing.SizeOunces = 8;
				}
				else
				{
					listing.SizeOunces = 12;
				}

				listing.MongoRoasterId = roaster.Id;
				listing.RoasterId = roaster.RoasterId;
				listing.DateAdded = DateTime.Now;

				listings.Add(listing);
			}

			return listings;
		}
	}
}
