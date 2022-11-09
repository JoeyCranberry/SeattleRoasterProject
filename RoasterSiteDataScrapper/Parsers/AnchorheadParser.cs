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
		public static List<BeanListing> ParseBeans(HtmlDocument shopHTML)
		{
			var shopItems = shopHTML.DocumentNode.Descendants()
				.Where(parentNode => parentNode.GetAttributeValue("class", "").Contains("product-grid-item"))
				.ToList();

			// collection__products
			// product-grid-item 

			List<BeanListing> listings = new List<BeanListing>();
			
			foreach (var item in shopItems)
			{
				BeanListing listing = new BeanListing();
				var imageNode = item.FirstChild;
				string imageUrl = imageNode.Descendants()
					.Where(node => node.GetAttributeValue("class", "").Contains("collection__image__top"))
					.First().GetAttributeValue("style", "");
				imageUrl = imageUrl.Substring(imageUrl.IndexOf("url("), imageUrl.IndexOf(");"));
				listing.ImageURL = imageUrl;
			}

			return listings;
		}
		// product-grid-item 
	}
}
