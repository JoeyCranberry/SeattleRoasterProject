using HtmlAgilityPack;
using RoasterBeansDataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoasterBeansDataAccess.Parsers
{
	public class DoteParser
	{
		public static List<BeanModel> ParseBeans(HtmlDocument shopHTML, RoasterModel roaster)
		{
			HtmlNode shopParent = shopHTML.DocumentNode.SelectSingleNode("//ul[contains(@class, 'coffee-l')]");
			List<HtmlNode> shopItems = shopParent.SelectNodes("./li").ToList();

			List<BeanModel> listings = new List<BeanModel>();

			foreach (HtmlNode productListing in shopItems)
			{
				BeanModel listing = new BeanModel();

				string imageURL = productListing.SelectSingleNode(".//img").GetAttributeValue("src", "");
				string productURL =  productListing.SelectSingleNode(".//a").GetAttributeValue("href", "");

				listing.ImageURL = imageURL;
				listing.ProductURL = productURL;
				
				string name = productListing.SelectSingleNode(".//h3").InnerText.Trim();
				listing.FullName = name;

				listing.AvailablePreground = false;
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

			return listings;
		}
	}
}
