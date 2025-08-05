using HtmlAgilityPack;
using RoasterBeansDataAccess.DataAccess;
using RoasterBeansDataAccess.Models;

namespace RoasterBeansDataAccess.Parsers;

public class BellinghamParser
{
    private const string blendsPageURL = "https://bellinghamcoffee.com/product-category/blends/";
    private static readonly List<string> excludedTerms = new() { "mug", "thermos" };

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

        var shopParent = shopHTML.DocumentNode?.SelectSingleNode("//ul[contains(@class, 'products')]");
        if (shopParent == null)
        {
            result.IsSuccessful = false;
            return result;
        }

        List<HtmlNode>? shopItems = shopParent?.SelectNodes(".//li")?.ToList();
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
                var productURL = productListing.SelectSingleNode(".//a").GetAttributeValue("href", "");

                listing.ProductURL = productURL;
                listing.ImageURL = imageURL;

                var name = productListing.SelectSingleNode(".//h2").FirstChild.InnerText.Trim();
                listing.FullName = name;

                var soldOutNode = productListing.SelectSingleNode(".//div[contains(@class, 'rey-soldout-badge')]");
                if (soldOutNode != null)
                {
                    listing.InStock = false;
                }

                listing.AvailablePreground = false;
                listing.SetOriginsFromName();
                listing.IsSingleOrigin = isSingleOrigin;
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