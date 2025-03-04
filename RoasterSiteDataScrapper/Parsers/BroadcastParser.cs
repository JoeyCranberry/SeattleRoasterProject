using HtmlAgilityPack;
using RoasterBeansDataAccess.DataAccess;
using RoasterBeansDataAccess.Models;

namespace RoasterBeansDataAccess.Parsers;

public class BroadcastParser
{
    private const string baseURL = "https://broadcastcoffeeroasters.com";

    public static async Task<ParseContentResult> ParseBeansForRoaster(RoasterModel roaster)
    {
        var shopContent = await PageContentAccess.GetPageContent(roaster.ShopURL);
        if (!string.IsNullOrEmpty(shopContent))
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(shopContent);

            return ParseBeans(htmlDoc, roaster);
        }

        return new ParseContentResult
        {
            IsSuccessful = false
        };
    }

    private static ParseContentResult ParseBeans(HtmlDocument shopHTML, RoasterModel roaster)
    {
        var result = new ParseContentResult();

        var shopParent = shopHTML.DocumentNode.SelectSingleNode("//ul[contains(@class, 'product-grid')]");
        if (shopParent == null)
        {
            result.IsSuccessful = false;
            return result;
        }

        List<HtmlNode>? shopItems = shopParent.SelectNodes(".//li")?.ToList();
        if (shopItems == null)
        {
            result.IsSuccessful = false;
            return result;
        }

        var listings = new List<BeanModel>();

        foreach (var productListing in shopItems)
        {
            var listing = new BeanModel();

            try
            {
                var imageURL = "https:" + productListing.SelectSingleNode(".//img").GetAttributeValue("src", "");
                var titleLinkNode = productListing.SelectSingleNode(".//a[contains(@class, 'full-unstyled-link')]");
                var productURL = baseURL + titleLinkNode.GetAttributeValue("href", "");

                listing.ProductURL = productURL;
                listing.ImageURL = imageURL;

                var name = titleLinkNode.InnerText.Trim();
                listing.FullName = name;
                // card-information
                var priceAndInfoNode = productListing.SelectSingleNode("//div[contains(@class, 'card-information')]");
                var regularPrice =
                    priceAndInfoNode.SelectSingleNode(".//span[contains(@class, 'price-item--regular')]");
                var price = "0";
                if (regularPrice != null)
                {
                    price = regularPrice.InnerText.Replace("From $", "").Trim();
                }
                // If item is on sale - reg price is place in a <s> elem instead of <span>
                else
                {
                    priceAndInfoNode.SelectSingleNode(".//s[contains(@class, 'price-item--regular')]").InnerText
                        .Replace("$", "")
                        .Replace("USD", "")
                        .Trim();
                }

                decimal parsedPrice;
                if (decimal.TryParse(price, out parsedPrice))
                {
                    listing.PriceBeforeShipping = parsedPrice;
                }

                listing.AvailablePreground = true;
                listing.SizeOunces = 12;
                listing.SetOriginsFromName();
                listing.SetDecafFromName();

                // Listings have sub captions that can say, Blend, Single-Origin, or decaf
                var addlInfo = priceAndInfoNode
                    .SelectSingleNode(".//div[contains(@class, 'caption-with-letter-spacing')]").InnerText.ToLower()
                    .Trim();
                switch (addlInfo)
                {
                    case "Decaf":
                        listing.IsDecaf = true;
                        break;
                    case "Single-Origin":
                        listing.IsSingleOrigin = true;
                        break;
                    case "Blend":
                        listing.IsSingleOrigin = false;
                        break;
                }


                listing.MongoRoasterId = roaster.Id;
                listing.RoasterId = roaster.RoasterId;
                listing.DateAdded = DateTime.Now;

                listings.Add(listing);
            }
            catch (Exception ex)
            {
                result.FailedParses++;
                result.exceptions.Add(ex);
            }
        }

        result.IsSuccessful = true;
        result.Listings = listings;

        return result;
    }
}