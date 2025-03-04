using HtmlAgilityPack;
using RoasterBeansDataAccess.DataAccess;
using RoasterBeansDataAccess.Models;

namespace RoasterBeansDataAccess.Parsers;

public class BlossomParser
{
    private const string baseUrl = "https://blossomcoffeeroasters.com";

    private const string blendsPageURL = "https://blossomcoffeeroasters.com/collections/blends";
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

        var shopParent = shopHTML.DocumentNode?.SelectSingleNode("//div[@id ='CollectionAjaxContent']");
        if (shopParent == null)
        {
            result.IsSuccessful = false;
            return result;
        }

        List<HtmlNode>? shopItems =
            shopParent.SelectNodes(".//div[contains(@class, 'grid-product__content')]")?.ToList();
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
                var imageURL = productListing.SelectSingleNode(".//div[contains(@class, 'grid__image-ratio')]")
                    .GetAttributeValue("style", "")
                    .Replace("180x", "360x")
                    .Replace("background-image: url(&quot;", "")
                    .Replace("&quot;);", "");
                var productURL = baseUrl + productListing
                    .SelectSingleNode(".//a[contains(@class, 'grid-product__link ')]").GetAttributeValue("href", "");

                listing.ProductURL = productURL;
                listing.ImageURL = imageURL;

                var name = productListing.SelectSingleNode(".//div[contains(@class, 'grid-product__title')]").InnerText
                    .Trim();
                listing.FullName = name;

                var priceNode = productListing.SelectSingleNode(".//div[contains(@class, 'grid-product__price')]");
                var price = priceNode.SelectSingleNode(".//span").InnerText.Replace("from $", "").Trim();
                decimal parsedPrice;
                if (decimal.TryParse(price, out parsedPrice))
                {
                    listing.PriceBeforeShipping = parsedPrice;
                }

                listing.AvailablePreground = false;
                listing.SetOriginsFromName();
                listing.IsSingleOrigin = isSingleOrigin;
                listing.SetProcessFromName();
                listing.SetDecafFromName();
                listing.SetOrganicFromName();

                listing.SizeOunces = 12;

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