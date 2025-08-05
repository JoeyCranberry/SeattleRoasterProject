using HtmlAgilityPack;
using RoasterBeansDataAccess.DataAccess;
using RoasterBeansDataAccess.Models;

namespace RoasterBeansDataAccess.Parsers;

internal class VahallaParser
{
    public static async Task<ParseContentResult> ParseBeansForRoaster(RoasterModel roaster)
    {
        // Add wait time to get page content since it takes a moment to load
        var shopContent = await PageContentAccess.GetPageContent(roaster.ShopURL, 0, 1000);
        if (!string.IsNullOrEmpty(shopContent))
        {
            HtmlDocument htmlDoc = new();
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
        ParseContentResult result = new();

        var shopParent = shopHTML.DocumentNode.SelectSingleNode("//div[contains(@class, 'w-grid')]");
        if (shopParent == null)
        {
            result.IsSuccessful = false;
            return result;
        }

        List<HtmlNode>? shopItems = shopParent.SelectNodes("./div[contains(@class, 'grid__item')]")?.ToList();
        if (shopItems == null)
        {
            result.IsSuccessful = false;
            return result;
        }

        List<BeanModel> listings = new();

        foreach (var productListing in shopItems)
        {
            BeanModel listing = new();

            try
            {
                var imageURL = productListing.SelectSingleNode(".//img").GetAttributeValue("src", "");

                var productURL = "https://www.valhallacoffee.com/home";

                listing.ImageURL = imageURL;
                listing.ProductURL = productURL;

                var name = productListing.SelectSingleNode(".//p[contains(@class, 'w-product-title')]").InnerText
                    .Trim();
                listing.FullName = name;

                var price = productListing.SelectSingleNode(".//p[contains(@class, 'product-price__wrapper')]")
                    .SelectSingleNode("./span").InnerText.Substring(0, 6).Replace("$", "").Trim();

                if (decimal.TryParse(price, out var parsedPrice))
                {
                    listing.PriceBeforeShipping = parsedPrice;
                }

                listing.AvailablePreground = true;
                listing.SizeOunces = 16;

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

        result.IsSuccessful = true;
        result.Listings = listings;

        return result;
    }
}