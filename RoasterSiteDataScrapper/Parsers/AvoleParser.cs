using HtmlAgilityPack;
using RoasterBeansDataAccess.DataAccess;
using RoasterBeansDataAccess.Models;
using SeattleRoasterProject.Core.Enums;

namespace RoasterBeansDataAccess.Parsers;

internal class AvoleParser
{
    private static readonly List<string> excludedTerms = new() { "jacket" };

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

        var shopParent = shopHTML.DocumentNode?.SelectSingleNode("//ul[contains(@class, 'S4WbK_')]");
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
            try
            {
                var listing = new BeanModel();

                var imageURL = productListing.SelectSingleNode(".//img").GetAttributeValue("src", "");
                var productURL = productListing.SelectSingleNode(".//a").GetAttributeValue("href", "");

                listing.ProductURL = productURL;
                listing.ImageURL = imageURL;

                var nameAndPriceNode =
                    productListing.SelectSingleNode(".//div[contains(@data-hook, 'not-image-container')]");
                var name = nameAndPriceNode.SelectSingleNode(".//h3").InnerText.Trim();
                listing.FullName = name;

                // Price can be out of stock
                // product-item-price-to-pay
                var priceInStockNode =
                    nameAndPriceNode.SelectSingleNode(".//span[contains(@data-hook, 'product-item-price-to-pay')]");
                var priceOutOfStockNode =
                    nameAndPriceNode.SelectSingleNode(".//span[contains(@data-hook, 'product-item-out-of-stock')]");

                if (priceInStockNode != null)
                {
                    var price = priceInStockNode.NextSibling.InnerText.Replace("$", "").Trim();

                    decimal parsedPrice;
                    if (decimal.TryParse(price, out parsedPrice))
                    {
                        listing.PriceBeforeShipping = parsedPrice;
                    }
                }
                else if (priceOutOfStockNode != null)
                {
                    listing.InStock = false;
                }
                else
                {
                    listing.InStock = false;
                    listing.PriceBeforeShipping = 0;
                }

                listing.AvailablePreground = true;
                // Avole is Ethopian only
                listing.Origins = new List<SourceLocation> { new(SourceCountry.Ethiopia) };
                listing.SetDecafFromName();

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