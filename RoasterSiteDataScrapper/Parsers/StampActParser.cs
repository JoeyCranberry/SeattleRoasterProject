using HtmlAgilityPack;
using RoasterBeansDataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoasterBeansDataAccess.Parsers
{
	internal class StampActParser
	{
		private const string baseURL = "https://stampactcoffee.com";

		public static List<BeanModel> ParseBeans(HtmlDocument shopHTML, RoasterModel roaster)
		{
			HtmlNode shopParent = shopHTML.DocumentNode.SelectSingleNode(".//div[@data-section-id='collection']")
				.SelectSingleNode(".//div[contains(@class, 'grid-uniform')]");
			List<HtmlNode> shopItems = shopParent.SelectNodes(".//div[contains(@class, 'grid__item')]").ToList();

			List<BeanModel> listings = new List<BeanModel>();

			foreach (HtmlNode productListing in shopItems)
			{
				BeanModel listing = new BeanModel();

				string imageURL;
				HtmlNode? imageNode = productListing.SelectSingleNode(".//a[@class='lazy-image']")
					?.SelectSingleNode("./img");
				if (imageNode != null)
				{
					imageURL = imageNode.GetAttributeValue("data-srcset", "");
					if(imageURL.Length > 2)
					{
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
				}

				string productURL = baseURL + productListing.SelectSingleNode(".//a").GetAttributeValue("href", "");

				listing.ProductURL = productURL;

				string name = productListing.SelectSingleNode(".//p[contains(@class, 'h6')]").InnerText.Trim();
				listing.FullName = name;

				string price = productListing.SelectSingleNode(".//span[@class='money']").InnerText.Replace("$", "").Trim();

				decimal parsedPrice;
				if (Decimal.TryParse(price, out parsedPrice))
				{
					listing.PriceBeforeShipping = parsedPrice;
				}

				listing.AvailablePreground = false;
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
