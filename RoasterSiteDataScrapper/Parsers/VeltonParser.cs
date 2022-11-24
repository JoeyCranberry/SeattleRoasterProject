using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using RoasterBeansDataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoasterBeansDataAccess.Parsers
{
	internal class VeltonParser
	{
		private const string baseURL = "https://www.veltonscoffee.com";

		public static List<BeanModel> ParseBeans(HtmlDocument shopHTML, RoasterModel roaster)
		{
			List<HtmlNode> shopItems = shopHTML.DocumentNode.SelectNodes(".//div[contains(@class, 'homepage-product')]").ToList();

			List<BeanModel> listings = new List<BeanModel>();

			foreach (HtmlNode productListing in shopItems)
			{
				BeanModel listing = new BeanModel();

				string imageURL = baseURL + productListing.SelectSingleNode(".//img").GetAttributeValue("src", "");

				string productURL = baseURL + productListing.SelectSingleNode(".//a").GetAttributeValue("href", "");

				listing.ImageURL = imageURL;
				listing.ProductURL = productURL;

				string name;
				HtmlNode h4NameNode = productListing.SelectSingleNode(".//a[contains(@class, 'h4')]");
				HtmlNode aNameNode = productListing.SelectSingleNode(".//h4");
				if(h4NameNode != null)
				{
					name = h4NameNode.InnerText.Trim();
					listing.FullName = name;
				}
				else if(aNameNode != null)
				{
					name = aNameNode.InnerText.Trim();
					listing.FullName = name;
				}

				string price = productListing.SelectSingleNode(".//span[contains(@class, 'CategoryProductPrice')]").InnerText.Replace("Price: 16 oz bag - $", "").Trim();

				decimal parsedPrice;
				if (Decimal.TryParse(price, out parsedPrice))
				{
					listing.PriceBeforeShipping = parsedPrice;
				}


				listing.AvailablePreground = true;
				listing.SizeOunces = 16;

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
