using HtmlAgilityPack;
using RoasterBeansDataAccess.DataAccess;
using RoasterBeansDataAccess.Models;

namespace RoasterBeansDataAccess.Parsers;

public class CafeAllegroParser
{
    private const string baseUrl = "https://seattleallegro.com";

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

        var shopParent = shopHTML.DocumentNode.SelectSingleNode("//div[contains(@class, 'card-list')]");
        if (shopParent == null)
        {
            result.IsSuccessful = false;
            return result;
        }

        List<HtmlNode>? shopItems = shopParent.SelectNodes(".//a")?.ToList();
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
                var imageNode = productListing.SelectSingleNode(".//img");
                var imageURL = imageNode.GetAttributeValue("data-srcset", "");
                if (string.IsNullOrEmpty(imageURL))
                {
                    imageURL = imageNode.GetAttributeValue("data-src", "");
                }

                imageURL = imageURL.Replace("{width}", "360");

                imageURL = imageURL.Substring(2, imageURL.Length - 2);
                var index = imageURL.IndexOf("//");
                if (index != -1)
                {
                    imageURL = "https://" + imageURL.Substring(0, index).Replace(" 180w, ", "");
                }
                else
                {
                    imageURL = "https://" + imageURL;
                }

                var productURL = baseUrl + productListing.GetAttributeValue("href", "");

                listing.ProductURL = productURL;
                listing.ImageURL = imageURL;

                var name = productListing.SelectSingleNode(".//h3").InnerText.Trim();
                listing.FullName = name;

                var price = productListing.SelectSingleNode(".//div[contains(@class, 'card__price')]").InnerText
                    .Replace("$", "");
                decimal parsedPrice;
                if (decimal.TryParse(price, out parsedPrice))
                {
                    listing.PriceBeforeShipping = parsedPrice;
                }

                listing.AvailablePreground = true;
                listing.SetOriginsFromName();
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
                result.Exceptions.Add(ex);
            }
        }

        result.IsSuccessful = true;
        result.Listings = listings;

        return result;
    }
}