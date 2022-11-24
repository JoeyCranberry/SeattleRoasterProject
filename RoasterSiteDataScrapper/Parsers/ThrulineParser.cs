using HtmlAgilityPack;
using RoasterBeansDataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoasterBeansDataAccess.Parsers
{
	internal class ThrulineParser
	{
		private const string baseURL = "https://www.thrulinecoffee.com";

		public static List<BeanModel> ParseBeans(HtmlDocument shopHTML, RoasterModel roaster)
		{
			HtmlNode shopParent = shopHTML.DocumentNode.SelectSingleNode(".//div[@id='CollectionAjaxContent']")
					.SelectSingleNode(".//div[contains(@class, 'grid--uniform')]");
			List<HtmlNode> shopItems = shopParent.SelectNodes(".//div[contains(@class, 'grid__item')]").ToList();

			List<BeanModel> listings = new List<BeanModel>();

			foreach (HtmlNode productListing in shopItems)
			{
				BeanModel listing = new BeanModel();

				string imageURL;
				HtmlNode imageNode = productListing.SelectSingleNode(".//img");
				if (imageNode != null)
				{
					imageURL = imageNode.GetAttributeValue("data-srcset", "");
					imageURL = imageURL.Substring(2, imageURL.Length - 2);
					int index = imageURL.IndexOf("//");
					if (index != -1)
					{
						imageURL = imageURL.Substring(0, index);
						imageURL = imageURL.Replace(" 180w,", "");
						imageURL = "https://" + imageURL;
						listing.ImageURL = imageURL;
					}
				}

				string productURL = baseURL + productListing.SelectSingleNode(".//a").GetAttributeValue("href", "");

				listing.ProductURL = productURL;

				string name = productListing.SelectSingleNode(".//div[contains(@class, 'grid-product__title')]").InnerText.Trim();
				listing.FullName = name;

				string price = productListing.SelectSingleNode(".//div[@class='grid-product__price']").InnerText.Replace("from $", "").Trim();

				decimal parsedPrice;
				if (Decimal.TryParse(price, out parsedPrice))
				{
					listing.PriceBeforeShipping = parsedPrice;
				}

				listing.AvailablePreground = true;
				listing.SizeOunces = 12;

				listing.SetRoastLevelFromName();
				listing.SetDecafFromName();
				listing.SetOriginsFromName();
				listing.SetProcessFromName();

				listing.MongoRoasterId = roaster.Id;
				listing.RoasterId = roaster.RoasterId;
				listing.DateAdded = DateTime.Now;

				listings.Add(listing);
			}

			return listings;
		}
	}
}
