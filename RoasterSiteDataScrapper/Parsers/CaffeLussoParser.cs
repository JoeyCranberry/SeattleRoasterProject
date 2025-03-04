using HtmlAgilityPack;
using RoasterBeansDataAccess.DataAccess;
using RoasterBeansDataAccess.Models;

namespace RoasterBeansDataAccess.Parsers;

public class CaffeLussoParser
{
    private const string baseURL = "https://caffelusso.com";
    private static readonly List<string> excludedTerms = new() { "gift", "sampler" };

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

        var shopParent = shopHTML.DocumentNode.SelectSingleNode("//div[contains(@class, 'product-list')]");
        if (shopParent == null)
        {
            result.IsSuccessful = false;
            return result;
        }

        List<HtmlNode>? shopItems = shopParent.SelectNodes(".//div[contains(@class, 'one-third')]")?.ToList();
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

                var productInfoNode =
                    productListing.SelectSingleNode(".//a[contains(@class, 'product-info__caption')]");
                var productURL = baseURL + productInfoNode.GetAttributeValue("href", "");

                listing.ProductURL = productURL;
                listing.ImageURL = imageURL;

                var name = productInfoNode.SelectSingleNode(".//span[contains(@class, 'title')]").InnerText.Trim();
                listing.FullName = name;

                var priceNode = productListing.SelectSingleNode(".//span[contains(@class, 'money')]");
                if (priceNode != null)
                {
                    var price = priceNode.InnerText.Replace("$", "");
                    decimal parsedPrice;
                    if (decimal.TryParse(price, out parsedPrice))
                    {
                        listing.PriceBeforeShipping = parsedPrice;
                    }
                }
                else
                {
                    listing.InStock = false;
                }

                listing.SetOriginsFromName();
                listing.SetDecafFromName();
                listing.SetRoastLevelFromName();

                listing.SizeOunces = 12M;

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