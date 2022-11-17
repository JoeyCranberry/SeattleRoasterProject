using HtmlAgilityPack;
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

		public static List<BeanModel> ParseBeans(HtmlDocument shopHTML, RoasterModel roaster)
		{
			HtmlNode shopParent = shopHTML.DocumentNode.SelectSingleNode("//ul[contains(@class, 'product-grid')]");
			List<HtmlNode> shopItems = shopParent.SelectNodes(".//li").ToList();

			List<BeanModel> listings = new List<BeanModel>();

			foreach (HtmlNode productListing in shopItems)
			{
				BeanModel listing = new BeanModel();

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

			return listings;
		}
	}
}
