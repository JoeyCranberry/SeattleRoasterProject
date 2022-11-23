using HtmlAgilityPack;
using RoasterBeansDataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoasterBeansDataAccess.Parsers
{
	internal class QEDParser
	{
		public static List<BeanModel> ParseBeans(HtmlDocument shopHTML, RoasterModel roaster)
		{
			HtmlNode shopParent = shopHTML.DocumentNode.SelectSingleNode(".//section[@data-hook='product-list']").SelectSingleNode(".//ul");
			List<HtmlNode> shopItems = shopParent.SelectNodes("./li").ToList();

			List<BeanModel> listings = new List<BeanModel>();

			foreach (HtmlNode productListing in shopItems)
			{
				BeanModel listing = new BeanModel();

				HtmlNode imageNode = productListing.SelectSingleNode(".//img");
				if(imageNode != null )
				{
					string imageURL = imageNode.GetAttributeValue("src", "");
					listing.ImageURL = imageURL;
				}
				
				string productURL = productListing.SelectSingleNode(".//a").GetAttributeValue("href", "");

				listing.ProductURL = productURL;

				string name = productListing.SelectSingleNode(".//h3").InnerText.Trim();
				listing.FullName = name;

				string price = productListing.SelectSingleNode(".//span[@data-hook='product-item-price-to-pay']").InnerText.Replace("$", "").Trim();

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
