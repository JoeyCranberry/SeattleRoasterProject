using HtmlAgilityPack;
using RoasterBeansDataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoasterBeansDataAccess.Parsers
{
	public class AvoleParser
	{
		private static List<string> excludedTerms = new List<string> { "jacket" };

		public static List<BeanModel> ParseBeans(HtmlDocument shopHTML, RoasterModel roaster)
		{
			HtmlNode shopParent = shopHTML.DocumentNode.SelectSingleNode("//ul[contains(@class, 'S4WbK_')]");
			List<HtmlNode> shopItems = shopParent.SelectNodes(".//li").ToList();

			List<BeanModel> listings = new List<BeanModel>();

			foreach (HtmlNode productListing in shopItems)
			{
				BeanModel listing = new BeanModel();

				string imageURL = productListing.SelectSingleNode(".//img").GetAttributeValue("src", "");
				string productURL = productListing.SelectSingleNode(".//a").GetAttributeValue("href", "");

				listing.ProductURL = productURL;
				listing.ImageURL = imageURL;

				HtmlNode nameAndPriceNode = productListing.SelectSingleNode(".//div[contains(@data-hook, 'not-image-container')]");
				string name = nameAndPriceNode.SelectSingleNode(".//h3").InnerText.Trim();
				listing.FullName = name;

				// Price can be out of stock
				// product-item-price-to-pay
				HtmlNode priceInStockNode = nameAndPriceNode.SelectSingleNode(".//span[contains(@data-hook, 'product-item-price-to-pay')]");
				HtmlNode priceOutOfStockNode = nameAndPriceNode.SelectSingleNode(".//span[contains(@data-hook, 'product-item-out-of-stock')]");

				if(priceInStockNode != null)
				{
					string price = priceInStockNode.NextSibling.InnerText.Replace("$", "").Trim();

					decimal parsedPrice;
					if (Decimal.TryParse(price, out parsedPrice))
					{
						listing.PriceBeforeShipping = parsedPrice;
					}
				}
				else if(priceOutOfStockNode != null)
				{
					listing.InStock = false;
				}
				else
				{
					listing.InStock = false;
					listing.PriceBeforeShipping = 0;
				}

				listing.AvailablePreground = true;
				// Avole is Ethopian only
				listing.CountriesOfOrigin = new List<Country>() { Country.ETHIOPIA };
				listing.SetDecafFromName();

				listing.MongoRoasterId = roaster.Id;
				listing.RoasterId = roaster.RoasterId;
				listing.DateAdded = DateTime.Now;

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
