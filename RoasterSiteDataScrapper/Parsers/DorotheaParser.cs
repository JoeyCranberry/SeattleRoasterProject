using HtmlAgilityPack;
using RoasterBeansDataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoasterBeansDataAccess.Parsers
{
	public class DorotheaParser
	{
		private const string baseURL = "https://dorotheacoffee.com/";

		public static List<BeanModel> ParseBeans(HtmlDocument shopHTML, RoasterModel roaster)
		{
			HtmlNode shopParent = shopHTML.DocumentNode.SelectSingleNode("//div[@data-section-id='featured-collection']");
			List<HtmlNode> shopItems = shopParent.SelectNodes(".//div[contains(@class, 'grid__item')]").ToList();

			List<BeanModel> listings = new List<BeanModel>();

			foreach (HtmlNode productListing in shopItems)
			{
				BeanModel listing = new BeanModel();

				string imageURL;
				HtmlNode imageNode = productListing.SelectSingleNode(".//img[contains(@class, 'product__img')]");
				if (imageNode != null)
				{
					imageURL = imageNode.GetAttributeValue("data-srcset", "");
					imageURL = imageURL.Substring(2, imageURL.Length - 2);
					int index = imageURL.IndexOf("//");
					if (index != -1)
					{
						imageURL = imageURL.Substring(0, index);
						imageURL = imageURL.Replace(" 150w,", "");
						imageURL = "https://" + imageURL;
						listing.ImageURL = imageURL;
					}
				}
				

				string productURL = baseURL + productListing.SelectSingleNode(".//a").GetAttributeValue("href", "");

				listing.ProductURL = productURL;
				

				string name = productListing.SelectSingleNode(".//p[@class='grid-link__title']").InnerText.Trim();
				listing.FullName = name;

				string price = productListing.SelectSingleNode(".//p[@class='grid-link__meta']").InnerText.Replace("From $", "").Trim();

				decimal parsedPrice;
				if (Decimal.TryParse(price, out parsedPrice))
				{
					listing.PriceBeforeShipping = parsedPrice;
				}

				HtmlNode soldOutNode = productListing.SelectSingleNode(".//span[contains(@class, 'badge--sold-out')]");
				if(soldOutNode != null)
				{
					listing.InStock = false;
				}

				listing.AvailablePreground = true;
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
