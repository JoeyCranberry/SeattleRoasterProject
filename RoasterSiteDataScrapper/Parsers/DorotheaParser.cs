using HtmlAgilityPack;
using RoasterBeansDataAccess.DataAccess;
using RoasterBeansDataAccess.Models;

namespace RoasterBeansDataAccess.Parsers;

public class DorotheaParser
{
    private const string baseURL = "https://dorotheacoffee.com/";

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

        var shopParent = shopHTML.DocumentNode.SelectSingleNode("//div[@class='grid-uniform grid-link__container']");
        if (shopParent == null)
        {
            result.IsSuccessful = false;
            return result;
        }

        List<HtmlNode>? shopItems = shopParent.SelectNodes("./div[contains(@class, 'grid__item')]")?.ToList();
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
                var imageNode = productListing.SelectSingleNode(".//img[contains(@class, 'product__img')]");
                if (imageNode != null)
                {
                    imageURL = imageNode.GetAttributeValue("data-srcset", "");
                    if (!string.IsNullOrEmpty(imageURL))
                    {
                        imageURL = imageURL.Substring(2, imageURL.Length - 2);
                        var index = imageURL.IndexOf("//");
                        if (index != -1)
                        {
                            imageURL = imageURL.Substring(0, index);
                            imageURL = imageURL.Replace(" 150w,", "");
                            imageURL = "https://" + imageURL;
                            listing.ImageURL = imageURL;
                        }
                    }
                }


                var productURL = baseURL + productListing.SelectSingleNode(".//a").GetAttributeValue("href", "");

                listing.ProductURL = productURL;


                var name = productListing.SelectSingleNode(".//p[@class='grid-link__title']").InnerText.Trim();
                listing.FullName = name;

                var price = productListing.SelectSingleNode(".//p[@class='grid-link__meta']").InnerText
                    .Replace("From $", "").Trim();

                decimal parsedPrice;
                if (decimal.TryParse(price, out parsedPrice))
                {
                    listing.PriceBeforeShipping = parsedPrice;
                }

                var soldOutNode = productListing.SelectSingleNode(".//span[contains(@class, 'badge--sold-out')]");
                if (soldOutNode != null)
                {
                    listing.InStock = false;
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
                result.exceptions.Add(ex);
            }
        }

        result.IsSuccessful = true;
        result.Listings = listings;

        return result;
    }
}