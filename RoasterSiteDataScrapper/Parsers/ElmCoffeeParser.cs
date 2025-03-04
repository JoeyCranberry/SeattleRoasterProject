using HtmlAgilityPack;
using RoasterBeansDataAccess.DataAccess;
using RoasterBeansDataAccess.Models;

namespace RoasterBeansDataAccess.Parsers;

public class ElmCoffeeParser
{
    private const string baseURL = "https://elmcoffeeroasters.com";

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

        var shopParent = shopHTML.DocumentNode.SelectSingleNode("//ul[@id='product-grid']");
        if (shopParent == null)
        {
            result.IsSuccessful = false;
            return result;
        }

        List<HtmlNode>? shopItems = shopParent.SelectNodes("./li")?.ToList();
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

                var productLinkNode = productListing.SelectSingleNode(".//a[@class='full-unstyled-link']");
                var productURL = baseURL + productLinkNode.GetAttributeValue("href", "");

                listing.ImageURL = imageURL;
                listing.ProductURL = productURL;


                var name = productLinkNode.InnerText.Trim();
                listing.FullName = name;

                var price = productListing.SelectSingleNode(".//span[contains(@class, 'price-item--regular')]")
                    .InnerText.Replace("$", "").Trim();

                decimal parsedPrice;
                if (decimal.TryParse(price, out parsedPrice))
                {
                    listing.PriceBeforeShipping = parsedPrice;
                }

                List<HtmlNode> tastingNotes = productListing.SelectNodes(".//span[@class='product-badge']").ToList();
                listing.TastingNotes = new List<string>();
                foreach (var tastingNote in tastingNotes)
                {
                    listing.TastingNotes.Add(tastingNote.InnerText.Trim());
                }

                listing.AvailablePreground = false;
                listing.SizeOunces = 10;

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
                result.exceptions.Add(ex);
            }
        }

        result.IsSuccessful = true;
        result.Listings = listings;

        return result;
    }
}