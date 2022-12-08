using HtmlAgilityPack;
using RoasterBeansDataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoasterBeansDataAccess.DataAccess;

namespace RoasterBeansDataAccess.Parsers
{
    public static class AnchorheadParser
	{
		private const string baseURL = "https://anchorheadcoffee.com";
		private static List<string> excludedTerms = new List<string>{ "choice", "sample", "tumbler", "shirt", "tee", "glass", "beanie", "gift card", "anchorhead - coffee supply co", "crewneck", "jacket" };

		public async static Task<ParseContentResult> ParseBeansForRoaster(RoasterModel roaster)
		{
			string? shopContent = await PageContentAccess.GetPageContent(roaster.ShopURL);
			if(!String.IsNullOrEmpty(shopContent))
			{
				HtmlDocument htmlDoc = new HtmlDocument();
				htmlDoc.LoadHtml(shopContent);

				return ParseBeans(htmlDoc, roaster);
			}

			return new ParseContentResult()
			{
				IsSuccessful = false
			};
		}

		private static ParseContentResult ParseBeans(HtmlDocument shopHTML, RoasterModel roaster)
		{
			ParseContentResult result = new ParseContentResult();

			HtmlNode? shopParent = shopHTML.DocumentNode?.SelectSingleNode("//div[contains(@class, 'collection__products')]")?.FirstChild;
			if(shopParent == null)
			{
				result.IsSuccessful = false;
				return result;
			}
			
			List<HtmlNode>? shopItems = shopParent.SelectNodes("//div[contains(@class, 'product-grid-item')]")?.ToList();
			if(shopItems == null)
			{
				result.IsSuccessful = false;
				return result;
			}

			shopItems.RemoveAll(child => child.ChildNodes.Count == 0);

			List<BeanModel> listings = new List<BeanModel>();
			
			foreach (HtmlNode productListing in shopItems)
			{
				BeanModel listing = new BeanModel();

				try
				{
					HtmlNode productImage = productListing.SelectSingleNode(".//a[contains(@class, 'lazy-image')]");
					string productURL = baseURL + productImage.GetAttributeValue("href", "");
					string imageURL = productImage.ChildNodes[1].GetAttributeValue("style", "");
					imageURL = imageURL
						.Replace("background-image: url(&quot;", "")
						.Replace("&quot;);", "");

					// Check for lazy loaded image
					if (String.IsNullOrEmpty(imageURL))
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
					listing.MongoRoasterId = roaster.Id;

					listing.SetOriginsFromName();
					listing.SetProcessFromName();
					listing.SetDecafFromName();

					listings.Add(listing);
				}
				catch(Exception ex) 
				{
					result.FailedParses++;
				}
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

			result.IsSuccessful = true;
			result.Listings = listings;

			return result;
		}	
	}
}
