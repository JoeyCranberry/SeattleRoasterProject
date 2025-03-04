using HtmlAgilityPack;
using RoasterBeansDataAccess.DataAccess;
using RoasterBeansDataAccess.Models;

namespace RoasterBeansDataAccess.Parsers;

public static class AnchorheadParser
{
    private const string baseURL = "https://anchorheadcoffee.com";

    private static readonly List<string> excludedTerms = new()
    {
        "choice", "sample", "tumbler", "shirt", "tee", "glass", "beanie", "gift card", "anchorhead - coffee supply co",
        "crewneck", "jacket", "ceramic cup", "hoodie", "hat", "bandanna", "sticker"
    };

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

        var shopParent = shopHTML.DocumentNode?.SelectSingleNode("//div[contains(@class, 'collection__products')]")
            ?.FirstChild;
        if (shopParent == null)
        {
            result.IsSuccessful = false;
            return result;
        }

        List<HtmlNode>? shopItems = shopParent.SelectNodes("//div[contains(@class, 'product-grid-item')]")?.ToList();
        if (shopItems == null)
        {
            result.IsSuccessful = false;
            return result;
        }

        shopItems.RemoveAll(child => child.ChildNodes.Count == 0);

        var listings = new List<BeanModel>();

        foreach (var productListing in shopItems)
        {
            var listing = new BeanModel();

            try
            {
                var productImage = productListing.SelectSingleNode(".//a[contains(@class, 'lazy-image')]");
                var productURL = baseURL + productImage.GetAttributeValue("href", "");
                var imageURL = productImage.ChildNodes[1].GetAttributeValue("style", "");
                imageURL = imageURL
                    .Replace("background-image: url(&quot;", "")
                    .Replace("&quot;);", "");

                // Check for lazy loaded image
                if (string.IsNullOrEmpty(imageURL))
                {
                    imageURL = "https://" + productImage.GetAttributeValue("style", "")
                        .Replace("padding-top:100%;", "")
                        .Replace("background-image:  url('//", "")
                        .Replace("_1x1", "_360x")
                        .Replace("');", "").Trim();
                }

                listing.ProductURL = productURL;
                listing.ImageURL = imageURL;

                var name = productListing.SelectSingleNode(".//p[contains(@class, 'product__grid__title')]").InnerText
                    .Replace("\n", "").Trim();
                var priceNode = productListing.SelectSingleNode(".//span[contains(@class, 'price')]");
                var price = priceNode.InnerText.Replace("From ", "").Replace("\n", "").Replace("$", "").Trim();

                decimal parsedPrice;
                if (decimal.TryParse(price, out parsedPrice))
                {
                    listing.PriceBeforeShipping = parsedPrice;
                    listing.SizeOunces = 10;
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