using HtmlAgilityPack;
using RoasterBeansDataAccess.DataAccess;
using RoasterBeansDataAccess.Models;

namespace RoasterBeansDataAccess.Parsers;

internal class UglyMugParser
{
    private const string baseURL = "https://uglymugseattle.com";

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

        var shopParent = shopHTML.DocumentNode.SelectSingleNode(".//div[contains(@class, 'ProductList-grid')]");
        if (shopParent == null)
        {
            result.IsSuccessful = false;
            return result;
        }

        List<HtmlNode>? shopItems = shopParent.SelectNodes("./div[contains(@class, 'ProductList-item')]")?.ToList();
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
                var imageURL = productListing.SelectSingleNode(".//img").GetAttributeValue("src", "");

                var productURL = baseURL + productListing.SelectSingleNode(".//a[@class='ProductList-item-link']")
                    .GetAttributeValue("href", "");

                listing.ImageURL = imageURL;
                listing.ProductURL = productURL;

                var name = productListing.SelectSingleNode(".//h1").InnerText.Trim();
                listing.FullName = name;

                var price = productListing.SelectSingleNode(".//div[@class='product-price']").InnerText.Replace("$", "")
                    .Trim();

                decimal parsedPrice;
                if (decimal.TryParse(price, out parsedPrice))
                {
                    listing.PriceBeforeShipping = parsedPrice;
                }

                var soldOutNode = productListing.SelectSingleNode(".//div[contains(@class, 'sold-out')]");
                if (soldOutNode != null)
                {
                    listing.InStock = false;
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

        result.IsSuccessful = true;
        result.Listings = listings;

        return result;
    }
}