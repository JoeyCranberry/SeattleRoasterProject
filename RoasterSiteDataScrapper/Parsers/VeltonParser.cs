using HtmlAgilityPack;
using RoasterBeansDataAccess.DataAccess;
using RoasterBeansDataAccess.Models;

namespace RoasterBeansDataAccess.Parsers;

internal class VeltonParser
{
    private const string baseURL = "https://www.veltonscoffee.com";

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

        List<HtmlNode>? shopItems = shopHTML.DocumentNode.SelectNodes(".//div[contains(@class, 'homepage-product')]")
            ?.ToList();
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
                var imageURL = baseURL + productListing.SelectSingleNode(".//img").GetAttributeValue("src", "");

                var productURL = baseURL + productListing.SelectSingleNode(".//a").GetAttributeValue("href", "");

                listing.ImageURL = imageURL;
                listing.ProductURL = productURL;

                string name;
                var h4NameNode = productListing.SelectSingleNode(".//a[contains(@class, 'h4')]");
                var aNameNode = productListing.SelectSingleNode(".//h4");
                if (h4NameNode != null)
                {
                    name = h4NameNode.InnerText.Trim();
                    listing.FullName = name;
                }
                else if (aNameNode != null)
                {
                    name = aNameNode.InnerText.Trim();
                    listing.FullName = name;
                }

                var price = productListing.SelectSingleNode(".//span[contains(@class, 'CategoryProductPrice')]")
                    .InnerText.Replace("Price: 16 oz bag - $", "").Trim();

                decimal parsedPrice;
                if (decimal.TryParse(price, out parsedPrice))
                {
                    listing.PriceBeforeShipping = parsedPrice;
                }


                listing.AvailablePreground = true;
                listing.SizeOunces = 16;

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
                result.Exceptions.Add(ex);
            }
        }

        result.IsSuccessful = true;
        result.Listings = listings;

        return result;
    }
}