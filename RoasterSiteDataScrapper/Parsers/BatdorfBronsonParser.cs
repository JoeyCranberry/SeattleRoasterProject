using HtmlAgilityPack;
using RoasterBeansDataAccess.DataAccess;
using RoasterBeansDataAccess.Models;

namespace RoasterBeansDataAccess.Parsers;

public class BatdorfBronsonParser
{
    private const string baseURL = "https://www.dancinggoats.com";

    private static readonly List<string> excludedTerms = new()
    {
        "choice", "sample", "tumbler", "shirt", "tee", "glass", "beanie", "gift card", "anchorhead - coffee supply co",
        "4oz"
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

        var shopParent = shopHTML.DocumentNode?.SelectSingleNode("//div[contains(@class, 'collection-products')]")
            ?.ChildNodes[1];
        if (shopParent == null)
        {
            result.IsSuccessful = false;
            return result;
        }

        List<HtmlNode>? shopItems = shopParent?.SelectNodes(".//div[contains(@class, 'o-layout__item')]")?.ToList();
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
                var productLinkNode = productListing.SelectSingleNode(".//a[contains(@class, 'product-link')]");
                var productURL = baseURL + productLinkNode.GetAttributeValue("href", "");
                var imgNode = productListing.SelectSingleNode(".//img");
                var imageURL = imgNode.GetAttributeValue("data-src", "");
                // Remove leading slashes
                if (!string.IsNullOrEmpty(imageURL))
                {
                    imageURL = "https:" + imageURL.Replace("{width}", "180");
                }

                listing.ProductURL = productURL;
                listing.ImageURL = imageURL;

                var name = productLinkNode.GetAttributeValue("title", "");
                var priceParentNode = productListing.SelectSingleNode(".//span[contains(@class, 'money')]");
                if (priceParentNode != null)
                {
                    var priceNode = priceParentNode.FirstChild;
                    if (priceNode != null)
                    {
                        var price = priceNode.InnerText.Replace("$", "");

                        decimal parsedPrice;
                        if (decimal.TryParse(price, out parsedPrice))
                        {
                            listing.PriceBeforeShipping = parsedPrice;
                        }
                    }
                }

                listing.FullName = name;

                listing.DateAdded = DateTime.Now;
                listing.RoasterId = roaster.RoasterId;
                listing.MongoRoasterId = roaster.Id;

                listing.SetOriginsFromName();
                listing.SetProcessFromName();
                listing.SetDecafFromName();
                listing.SetOrganicFromName();
                listing.SetFairTradeFromName();

                if (name.Contains("4oz"))
                {
                    listing.SizeOunces = 4;
                }
                else
                {
                    listing.SizeOunces = 12;
                }

                listing.AvailablePreground = true;

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