using HtmlAgilityPack;
using RoasterBeansDataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoasterBeansDataAccess.Parsers
{
	public class CafeAllegroParser
	{
		private const string baseUrl = "https://seattleallegro.com";
		public static List<BeanModel> ParseBeans(HtmlDocument shopHTML, RoasterModel roaster)
		{
			HtmlNode shopParent = shopHTML.DocumentNode.SelectSingleNode("//div[contains(@class, 'card-list')]");
			List<HtmlNode> shopItems = shopParent.SelectNodes(".//a").ToList();

			List<BeanModel> listings = new List<BeanModel>();

			foreach (HtmlNode productListing in shopItems)
			{
				BeanModel listing = new BeanModel();

				HtmlNode imageNode = productListing.SelectSingleNode(".//img");
				string imageURL = imageNode.GetAttributeValue("data-srcset", "");
				if(String.IsNullOrEmpty(imageURL)) 
				{
					imageURL = imageNode.GetAttributeValue("data-src", "");
				}
				imageURL = imageURL.Replace("{width}", "360");

				imageURL = imageURL.Substring(2, imageURL.Length- 2);
				int index = imageURL.IndexOf("//");
				if(index != -1)
				{
					imageURL = "https://" + imageURL.Substring(0, index).Replace(" 180w, ", "");
				}
				else
				{
					imageURL = "https://" + imageURL;
				}

				string productURL = baseUrl + productListing.GetAttributeValue("href", "");

				listing.ProductURL = productURL;
				listing.ImageURL = imageURL;

				string name = productListing.SelectSingleNode(".//h3").InnerText.Trim();
				listing.FullName = name;

				string price = productListing.SelectSingleNode(".//div[contains(@class, 'card__price')]").InnerText.Replace("$", "");
				decimal parsedPrice;
				if (Decimal.TryParse(price, out parsedPrice))
				{
					listing.PriceBeforeShipping = parsedPrice;
				}

				listing.AvailablePreground = true;
				listing.SetOriginsFromName();
				listing.SetProcessFromName();
				listing.SetDecafFromName();
				listing.SetOrganicFromName();

				listing.SizeOunces = 12;

				listing.MongoRoasterId = roaster.Id;
				listing.RoasterId = roaster.RoasterId;
				listing.DateAdded = DateTime.Now;

				listings.Add(listing);
			}

			return listings;
		}
	}
}
