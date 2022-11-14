using HtmlAgilityPack;
using RoasterBeansDataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoasterBeansDataAccess.Parsers
{
	public class BatdorfBronsonParser
	{
		private const string baseURL = "https://www.dancinggoats.com";
		private static List<string> excludedTerms = new List<string> { "choice", "sample", "tumbler", "shirt", "tee", "glass", "beanie", "gift card", "anchorhead - coffee supply co" };

		public static List<BeanModel> ParseBeans(HtmlDocument shopHTML, RoasterModel roaster)
		{
			HtmlNode shopParent = shopHTML.DocumentNode.SelectSingleNode("//div[contains(@class, 'collection-products')]").ChildNodes[1];
			List<HtmlNode> shopItems = shopParent.SelectNodes(".//div[contains(@class, 'o-layout__item')]").ToList();
			shopItems.RemoveAll(child => child.ChildNodes.Count == 0);

			List<BeanModel> listings = new List<BeanModel>();

			foreach (HtmlNode productListing in shopItems)
			{
				BeanModel listing = new BeanModel();

				HtmlNode productLinkNode = productListing.SelectSingleNode(".//a[contains(@class, 'product-link')]");
				string productURL = baseURL + productLinkNode.GetAttributeValue("href", "");
				HtmlNode imgNode = productListing.SelectSingleNode(".//img");
				string imageURL = imgNode.GetAttributeValue("data-src", "");
				// Remove leading slashes
				if(!String.IsNullOrEmpty(imageURL))
				{
					imageURL = "https:" + imageURL.Replace("{width}", "180");
				}

				listing.ProductURL = productURL;
				listing.ImageURL = imageURL;

				string name = productLinkNode.GetAttributeValue("title", "");
				string price = productListing.SelectSingleNode(".//span[contains(@class, 'money')]").FirstChild.InnerText.Replace("$", "");

				decimal parsedPrice;
				if (Decimal.TryParse(price, out parsedPrice))
				{
					listing.PriceBeforeShipping = parsedPrice;
				}
				listing.FullName = name;

				listing.DateAdded = DateTime.Now;
				listing.RoasterId = roaster.RoasterId;

				listing.SetOriginsFromName();
				listing.SetProcessFromName();
				listing.SetDecafFromName();
				listing.SetOrganicFromName();
				listing.SetFairTradeFromName();
				listing.SizeOunces = 12;
				listing.AvailablePreground = true;

				listings.Add(listing);
			}

			// Remove any excluded terms
			foreach (var product in listings)
			{
				foreach (string term in excludedTerms)
				{
					if (product.FullName.ToLower().Contains(term))
					{
						product.IsExcluded = true;
					}
				}
			}

			return listings;
		}
	}
}
