using HtmlAgilityPack;
using RoasterBeansDataAccess.DataAccess;
using RoasterBeansDataAccess.Models;

namespace RoasterBeansDataAccess.Parsers;

public class CloudCityParser
{
    private const string baseUrl = "https://www.cloudcitycoffeeroasting.com";

    private const string blendsPageURL = "https://www.cloudcitycoffeeroasting.com/collections/blends";
    private static readonly List<string> excludedTerms = new() { "subscription", "sampler" };

    public static async Task<ParseContentResult> ParseBeansForRoaster(RoasterModel roaster)
    {
        var overallResult = new ParseContentResult
        {
            Listings = new List<BeanModel>(),
            IsSuccessful = false
        };

        overallResult = await ParsePage(overallResult, roaster.ShopURL, roaster, true);
        overallResult = await ParsePage(overallResult, blendsPageURL, roaster, false);

        return overallResult;
    }

    private static async Task<ParseContentResult> ParsePage(ParseContentResult overallResult, string pageURL,
        RoasterModel roaster, bool isSingleOrigin)
    {
        var shopContent = await PageContentAccess.GetPageContent(pageURL);
        if (!string.IsNullOrEmpty(shopContent))
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(shopContent);

            var parseResult = ParseBeans(htmlDoc, roaster, isSingleOrigin);

            if (parseResult.IsSuccessful && parseResult.Listings != null && overallResult.Listings != null)
            {
                overallResult.Listings.AddRange(parseResult.Listings);
                overallResult.FailedParses += parseResult.FailedParses;
                overallResult.IsSuccessful = true;
            }
        }

        return overallResult;
    }

    private static ParseContentResult ParseBeans(HtmlDocument shopHTML, RoasterModel roaster, bool isSingleOrigin)
    {
        var result = new ParseContentResult();

        var shopParent =
            shopHTML.DocumentNode.SelectSingleNode("//div[contains(@class, 'collection-page__product-list')]");
        List<HtmlNode> shopItems = shopParent.SelectNodes(".//article").ToList();

        var listings = new List<BeanModel>();

        foreach (var productListing in shopItems)
        {
            var listing = new BeanModel();

            try
            {
                var imageURL = "https:" + productListing.SelectSingleNode(".//img").GetAttributeValue("src", "");
                var productURL = baseUrl + productListing
                    .SelectSingleNode(".//a[contains(@class, 'product-item__image-link')]")
                    .GetAttributeValue("href", "");

                listing.ProductURL = productURL;
                listing.ImageURL = imageURL;

                var name = productListing.SelectSingleNode(".//h3").SelectSingleNode(".//a").InnerText
                    .Replace("🌱*NEW*", "").Trim();
                name = name.Replace("🌱 ", "").Replace("*NEW*", "").Trim();

                listing.FullName = name;

                var price = productListing.SelectSingleNode(".//span[contains(@class, 'money')]").InnerText
                    .Replace("$", "").Trim();

                decimal parsedPrice;
                if (decimal.TryParse(price, out parsedPrice))
                {
                    listing.PriceBeforeShipping = parsedPrice;
                }

                listing.AvailablePreground = true;
                listing.SizeOunces = 12;

                listing.SetOriginsFromName();
                listing.IsSingleOrigin = isSingleOrigin;

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
                result.exceptions.Add(ex);
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