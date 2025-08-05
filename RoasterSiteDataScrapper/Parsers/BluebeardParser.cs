using HtmlAgilityPack;
using RoasterBeansDataAccess.DataAccess;
using RoasterBeansDataAccess.Models;

namespace RoasterBeansDataAccess.Parsers;

public class BluebeardParser
{
    private const string baseURL = "https://bluebeardcoffee.com";
    private static readonly List<string> excludedTerms = new() { "instant", "subscription", "gift" };

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

        var shopParent =
            shopHTML.DocumentNode?.SelectSingleNode("//div[contains(@class, 'mainCollectionProductGrid')]");
        if (shopParent == null)
        {
            result.IsSuccessful = false;
            return result;
        }

        List<HtmlNode>? shopItems = shopParent.SelectNodes(".//a").ToList();
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
                var productURL = baseURL + productListing.GetAttributeValue("href", "");

                listing.ProductURL = productURL;
                listing.ImageURL = imageURL;

                var name = productListing.SelectSingleNode(".//h3").ChildNodes[1].InnerText.Trim();
                listing.FullName = name;

                var priceNode = productListing.SelectSingleNode(".//span[contains(@class, 'price-regular')]");

                if (priceNode != null)
                {
                    var price = priceNode.InnerText.Replace("From $", "").Trim();

                    if (!string.IsNullOrEmpty(price))
                    {
                        decimal parsedPrice;
                        if (decimal.TryParse(price, out parsedPrice))
                        {
                            listing.PriceBeforeShipping = parsedPrice;
                        }
                    }
                }

                listing.AvailablePreground = true;

                listing.SetOriginsFromName();
                listing.SetDecafFromName();

                listing.SizeOunces = 12M;

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