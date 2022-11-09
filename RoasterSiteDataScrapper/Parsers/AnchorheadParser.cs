using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoasterBeansDataAccess.Parsers
{
	public static class AnchorheadParser
	{
		private const string baseURL = "https://anchorheadcoffee.com";
		private static List<string> excludedTerms = new List<string>{ "choice", "sample", "tumbler", "shirt", "tee", "glass", "beanie", "gift card", "anchorhead - coffee supply co" };

		public static List<BeanListing> ParseBeans(HtmlDocument shopHTML, Roaster roaster)
		{
			HtmlNode shopParent = shopHTML.DocumentNode.SelectSingleNode("//div[contains(@class, 'collection__products')]").FirstChild;
			List<HtmlNode> shopItems = shopParent.SelectNodes("//div[contains(@class, 'product-grid-item')]").ToList();
			shopItems.RemoveAll(child => child.ChildNodes.Count == 0);

			List<BeanListing> listings = new List<BeanListing>();
			
			foreach (HtmlNode productListing in shopItems)
			{
				BeanListing listing = new BeanListing();

				HtmlNode productImage = productListing.SelectSingleNode(".//a[contains(@class, 'lazy-image')]");
				string productURL = baseURL + productImage.GetAttributeValue("href", "");
				string imageURL = productImage.ChildNodes[1].GetAttributeValue("style", "");
				imageURL = imageURL
					.Replace("background-image: url(&quot;", "")
					.Replace("&quot;);", "");

				// Check for lazy loaded image
				if(String.IsNullOrEmpty(imageURL)) 
                {
					imageURL = "https://" + productImage.GetAttributeValue("style", "")
						.Replace("padding-top:100%;", "")
						.Replace("background-image:  url('//", "")
						.Replace("_1x1", "_360x")
						.Replace("');", "").Trim();
				}

				listing.ProductURL = productURL;
				listing.ImageURL = imageURL;

				string name = productListing.SelectSingleNode(".//p[contains(@class, 'product__grid__title')]").InnerText.Replace("\n", "").Trim();
				HtmlNode priceNode = productListing.SelectSingleNode(".//span[contains(@class, 'price')]");
				string price = priceNode.InnerText.Replace("From ", "").Replace("\n", "").Replace("$", "").Trim();

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

				listings.Add(listing);
			}

			// Remove any excluded terms
			foreach(var product in listings)
            {
				foreach(string term in excludedTerms)
                {
					if(product.FullName.ToLower().Contains(term))
                    {
						product.IsExcluded = true;
					}
                }
            }

			return listings;
		}	
	}
}
