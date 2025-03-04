using HtmlAgilityPack;
using RoasterBeansDataAccess.DataAccess;
using RoasterBeansDataAccess.Models;

namespace RoasterBeansDataAccess.Parsers;

internal class ThrulineParser
{
    private const string baseURL = "https://www.thrulinecoffee.com";

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

        var shopParent = shopHTML.DocumentNode.SelectSingleNode(".//div[@id='CollectionAjaxContent']")
            ?.SelectSingleNode(".//div[contains(@class, 'grid--uniform')]");
        if (shopParent == null)
        {
            result.IsSuccessful = false;
            return result;
        }

        List<HtmlNode>? shopItems = shopParent.SelectNodes(".//div[contains(@class, 'grid__item')]")?.ToList();
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
                string imageURL;
                var imageNode = productListing.SelectSingleNode(".//img");
                if (imageNode != null)
                {
                    imageURL = imageNode.GetAttributeValue("data-srcset", "");
                    imageURL = imageURL.Substring(2, imageURL.Length - 2);
                    var index = imageURL.IndexOf("//");
                    if (index != -1)
                    {
                        imageURL = imageURL.Substring(0, index);
                        imageURL = imageURL.Replace(" 180w,", "");
                        imageURL = "https://" + imageURL;
                        listing.ImageURL = imageURL;
                    }
                }

                var productURL = baseURL + productListing.SelectSingleNode(".//a").GetAttributeValue("href", "");

                listing.ProductURL = productURL;

                var name = productListing.SelectSingleNode(".//div[contains(@class, 'grid-product__title')]").InnerText
                    .Trim();
                listing.FullName = name;

                var price = productListing.SelectSingleNode(".//div[@class='grid-product__price']").InnerText
                    .Replace("from $", "").Trim();

                if (decimal.TryParse(price, out var parsedPrice))
                {
                    listing.PriceBeforeShipping = parsedPrice;
                }

                listing.AvailablePreground = true;
                listing.SizeOunces = 12;

                listing.SetRoastLevelFromName();
                listing.SetDecafFromName();
                listing.SetOriginsFromName();
                listing.SetProcessFromName();

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

        result.IsSuccessful = true;
        result.Listings = listings;

        return result;
    }
}