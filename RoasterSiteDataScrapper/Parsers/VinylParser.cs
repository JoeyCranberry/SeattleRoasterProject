using System.Globalization;
using HtmlAgilityPack;
using RoasterBeansDataAccess.DataAccess;
using RoasterBeansDataAccess.Models;

namespace RoasterBeansDataAccess.Parsers;

internal class VinylParser
{
    private const string baseURL = "https://www.vinylcoffeeroasters.com";

    private static readonly List<string> excludedTerms = new()
        { "mug", "gift", "sticked", "t-shirt", "sticker", "subscription" };

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

        var shopParent = shopHTML.DocumentNode.SelectSingleNode(".//div[@class='products-flex-container']")
            ?.ChildNodes[1];
        if (shopParent == null)
        {
            result.IsSuccessful = false;
            return result;
        }

        List<HtmlNode>? shopItems = shopParent.SelectNodes(".//div[contains(@class, 'grid-item')]")?.ToList();
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
                var imageURL = productListing.SelectSingleNode(".//img").GetAttributeValue("data-src", "");

                var productURL = baseURL + productListing.SelectSingleNode(".//a").GetAttributeValue("href", "");

                listing.ImageURL = imageURL;
                listing.ProductURL = productURL;

                var name = productListing.SelectSingleNode(".//div[contains(@class, 'grid-title')]").InnerText.Trim();
                var textInfo = new CultureInfo("en-US", false).TextInfo;
                name = textInfo.ToTitleCase(name.ToLower());

                listing.FullName = name;

                var price = productListing.SelectSingleNode(".//div[contains(@class, 'product-price')]").InnerText
                    .Replace("from $", "").Trim();

                decimal parsedPrice;
                if (decimal.TryParse(price, out parsedPrice))
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
            catch (Exception ex)
            {
                result.FailedParses++;
                result.Exceptions.Add(ex);
            }
        }

        // Remove any excluded terms
        foreach (var product in listings)
        {
            foreach (var term in excludedTerms)
            {
                if (product.FullName.ToLower().Contains(term))
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