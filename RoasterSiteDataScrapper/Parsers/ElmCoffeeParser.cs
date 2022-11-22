using HtmlAgilityPack;
using RoasterBeansDataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoasterBeansDataAccess.Parsers
{
	public class ElmCoffeeParser
	{
		private const string baseURL = "https://elmcoffeeroasters.com";

		public static List<BeanModel> ParseBeans(HtmlDocument shopHTML, RoasterModel roaster)
		{
			HtmlNode shopParent = shopHTML.DocumentNode.SelectSingleNode("//ul[@id='product-grid']");
			List<HtmlNode> shopItems = shopParent.SelectNodes("./li").ToList();

			List<BeanModel> listings = new List<BeanModel>();

			foreach (HtmlNode productListing in shopItems)
			{
				BeanModel listing = new BeanModel();

				string imageURL = "https:" + productListing.SelectSingleNode(".//img").GetAttributeValue("src", "");

				HtmlNode productLinkNode = productListing.SelectSingleNode(".//a[@class='full-unstyled-link']");
				string productURL = baseURL + productLinkNode.GetAttributeValue("href", "");

				listing.ImageURL= imageURL;
				listing.ProductURL = productURL;


				string name = productLinkNode.InnerText.Trim();
				listing.FullName = name;

				string price = productListing.SelectSingleNode(".//span[contains(@class, 'price-item--regular')]").InnerText.Replace("$", "").Trim();

				decimal parsedPrice;
				if (Decimal.TryParse(price, out parsedPrice))
				{
					listing.PriceBeforeShipping = parsedPrice;
				}

				List<HtmlNode> tastingNotes = productListing.SelectNodes(".//span[@class='product-badge']").ToList();
				listing.TastingNotes = new List<string>();
				foreach (HtmlNode tastingNote in tastingNotes)
				{
					listing.TastingNotes.Add(tastingNote.InnerText.Trim());
				}

				listing.AvailablePreground = false;
				listing.SizeOunces = 10;

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
