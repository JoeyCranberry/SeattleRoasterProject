using HtmlAgilityPack;
using RoasterBeansDataAccess.DataAccess;
using RoasterBeansDataAccess.Models;

namespace RoasterBeansDataAccess.Parsers;

internal class VictrolaParser
{
    private const string baseURL = "https://www.victrolacoffee.com";

    private const string page2URL = "https://www.victrolacoffee.com/collections/all-coffee-offerings?page=2";
    private static readonly List<string> excludedTerms = new() { "steeped", "subscription" };

    public static async Task<ParseContentResult> ParseBeansForRoaster(RoasterModel roaster)
    {
        var overallResult = new ParseContentResult
        {
            Listings = new List<BeanModel>(),
            IsSuccessful = false
        };

        overallResult = await ParsePage(overallResult, roaster.ShopURL, roaster);
        overallResult = await ParsePage(overallResult, page2URL, roaster);

        return overallResult;
    }

    private static async Task<ParseContentResult> ParsePage(ParseContentResult overallResult, string pageURL,
        RoasterModel roaster)
    {
        var shopContent = await PageContentAccess.GetPageContent(pageURL);
        if (!string.IsNullOrEmpty(shopContent))
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(shopContent);

            var parseResult = ParseBeans(htmlDoc, roaster);

            if (parseResult.IsSuccessful && parseResult.Listings != null && overallResult.Listings != null)
            {
                overallResult.Listings.AddRange(parseResult.Listings);
                overallResult.FailedParses += parseResult.FailedParses;
                overallResult.IsSuccessful = true;
            }
        }

        return overallResult;
    }

    private static ParseContentResult ParseBeans(HtmlDocument shopHTML, RoasterModel roaster)
    {
        var result = new ParseContentResult();

        var shopParent = shopHTML.DocumentNode.SelectSingleNode("//div[contains(@class, 'collection-products')]");
        if (shopParent == null)
        {
            result.IsSuccessful = false;
            return result;
        }

        List<HtmlNode>? shopItems = shopParent.SelectNodes("./div[@class='product-list-item']")?.ToList();
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
                var productURL = baseURL + productListing.SelectSingleNode(".//a").GetAttributeValue("href", "");

                listing.ProductURL = productURL;
                listing.ImageURL = imageURL;

                var name = productListing.SelectSingleNode(".//h3").InnerText.Trim();
                listing.FullName = name;

                var price = productListing.SelectSingleNode(".//p[@class='product-list-item-price']").InnerText
                    .Replace("$ ", "").Trim();

                decimal parsedPrice;
                if (decimal.TryParse(price, out parsedPrice))
                {
                    listing.PriceBeforeShipping = parsedPrice;
                }

                listing.SizeOunces = 12;
                listing.IsAboveFairTradePricing = true;

                listing.AvailablePreground = false;
                listing.SetOriginsFromName();

                listing.SetProcessFromName();
                listing.SetDecafFromName();
                listing.SetOrganicFromName();

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