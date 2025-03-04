using HtmlAgilityPack;
using RoasterBeansDataAccess.DataAccess;
using RoasterBeansDataAccess.Models;
using SeattleRoasterProject.Core.Enums;

namespace RoasterBeansDataAccess.Parsers;

public class CaffeLadroParser
{
    private static readonly List<string> excludedTerms = new() { "subscription", "box", "cup", "steeped", "5lb" };

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

        var shopParent = shopHTML.DocumentNode.SelectSingleNode("//div[contains(@class, 'fluidContainer')]");
        if (shopParent == null)
        {
            result.IsSuccessful = false;
            return result;
        }

        List<HtmlNode>? shopItems = shopParent.SelectNodes(".//div[contains(@class, 'tileContent')]")?.ToList();
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
                var productURL = productListing.SelectSingleNode(".//a[contains(@class, 'nextProdThumb')]")
                    .GetAttributeValue("href", "");
                var imageURL = "https:" + productListing.SelectSingleNode(".//img").GetAttributeValue("src", "");

                listing.ProductURL = productURL;
                listing.ImageURL = imageURL;

                var name = productListing.SelectSingleNode(".//a[contains(@class, 'nextProdName')]").InnerText
                    .Replace("12oz", "");
                listing.FullName = name;

                var price = productListing.SelectSingleNode(".//div[contains(@class, 'nextPrice')]")
                    .SelectSingleNode(".//b").InnerText.Replace("$", "");

                if (!string.IsNullOrEmpty(price))
                {
                    decimal parsedPrice;
                    if (decimal.TryParse(price, out parsedPrice))
                    {
                        listing.PriceBeforeShipping = parsedPrice;
                    }
                }

                listing.AvailablePreground = true;

                listing.SetOriginsFromName();
                listing.SetDecafFromName();

                var addlInfo = productListing.SelectSingleNode(".//div[contains(@class, 'nextShortDesc')]")?.InnerText
                    .ToLower();
                if (!string.IsNullOrEmpty(addlInfo))
                {
                    if (addlInfo.Contains("fair trade"))
                    {
                        listing.IsFairTradeCertified = true;
                    }

                    if (addlInfo.Contains("organic"))
                    {
                        listing.OrganicCerification = OrganicCertification.Certified_Organic;
                    }
                }

                listing.SizeOunces = 12M;

                listing.MongoRoasterId = roaster.Id;
                listing.RoasterId = roaster.RoasterId;
                listing.DateAdded = DateTime.Now;

                listings.Add(listing);
            }
            catch (Exception ex)
            {
                if (result.exceptions == null)
                {
                    result.exceptions = new List<Exception>();
                }

                result.exceptions.Add(ex);
                result.FailedParses++;
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