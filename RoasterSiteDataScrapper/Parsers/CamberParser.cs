using HtmlAgilityPack;
using RoasterBeansDataAccess.DataAccess;
using RoasterBeansDataAccess.Models;

namespace RoasterBeansDataAccess.Parsers;

public class CamberParser
{
    private static readonly List<string> excludedTerms = new() { "sample", "rotating", "gift" };

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

        var shopParent = shopHTML.DocumentNode.SelectSingleNode("//ul[contains(@class, 'products')]");
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
            var listing = new BeanModel();

            try
            {
                var imageURL = productListing.SelectSingleNode(".//img").GetAttributeValue("src", "");
                var productURL = productListing.SelectSingleNode(".//a").GetAttributeValue("href", "");

                listing.ProductURL = productURL;
                listing.ImageURL = imageURL;

                var name = productListing.SelectSingleNode(".//h2").InnerText.Replace("<br>", "");
                listing.FullName = name;

                var price = productListing
                    .SelectSingleNode(".//span[contains(@class, 'woocommerce-Price-currencySymbol')]").NextSibling
                    .InnerText.Trim();

                decimal parsedPrice;
                if (decimal.TryParse(price, out parsedPrice))
                {
                    listing.PriceBeforeShipping = parsedPrice;
                }

                listing.AvailablePreground = true;
                listing.SizeOunces = 12;
                listing.SetOriginsFromName();
                listing.SetDecafFromName();
                listing.SetProcessFromName();
                listing.SetOrganicFromName();
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