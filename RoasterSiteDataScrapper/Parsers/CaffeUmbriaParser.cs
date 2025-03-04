using System.Globalization;
using HtmlAgilityPack;
using RoasterBeansDataAccess.DataAccess;
using RoasterBeansDataAccess.Models;
using SeattleRoasterProject.Core.Enums;

namespace RoasterBeansDataAccess.Parsers;

public class CaffeUmbriaParser
{
    private const string baseURL = "https://caffeumbria.com";

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

        var shopParent = shopHTML.DocumentNode.SelectSingleNode("//div[contains(@class, 'products')]");
        if (shopParent == null)
        {
            result.IsSuccessful = false;
            return result;
        }

        List<HtmlNode>? shopItems = shopParent.SelectNodes(".//div[@class = 'product col col--1of3']")?.ToList();
        if (shopItems == null)
        {
            result.IsSuccessful = false;
            return result;
        }

        var featuredNode = shopHTML.DocumentNode.SelectSingleNode("//div[contains(@class, 'products--featured')]")
            ?.SelectSingleNode("./div");
        if (featuredNode != null)
        {
            shopItems.Add(featuredNode);
        }

        var listings = new List<BeanModel>();

        var textInfo = new CultureInfo("en-US", false).TextInfo;
        foreach (var productListing in shopItems)
        {
            var listing = new BeanModel();

            try
            {
                var imageURL = productListing.SelectSingleNode(".//div[@class='product__image']")
                    .GetAttributeValue("style", "")
                    .Replace("background-image: url(", "")
                    .Replace(");", "");
                imageURL = "https:" + imageURL;

                var productURL = baseURL + productListing.SelectSingleNode(".//a").GetAttributeValue("href", "");

                listing.ProductURL = productURL;
                listing.ImageURL = imageURL;

                var titleNode = productListing.SelectSingleNode(".//h2[contains(@class, 'product__title')]");
                var titleSpanNode = titleNode.SelectSingleNode(".//span");
                string name;
                if (titleSpanNode != null)
                {
                    name = titleSpanNode.InnerText;
                }
                else
                {
                    name = titleNode.InnerText;
                }

                name = textInfo.ToTitleCase(name.ToLower());
                listing.FullName = name;

                listing.AvailablePreground = true;

                var addlInfo = productListing.SelectSingleNode(".//div[contains(@class, 'product__info-items')]")
                    ?.InnerText.ToLower();
                if (addlInfo != null)
                {
                    if (addlInfo.Contains("blend"))
                    {
                        listing.IsSingleOrigin = false;
                    }

                    if (addlInfo.Contains("light"))
                    {
                        listing.RoastLevel = RoastLevel.Light;
                    }
                    else if (addlInfo.Contains("medium"))
                    {
                        listing.RoastLevel = RoastLevel.Medium;
                    }
                    else if (addlInfo.Contains("dark"))
                    {
                        listing.RoastLevel = RoastLevel.Dark;
                    }

                    if (addlInfo.Contains("decaf"))
                    {
                        listing.IsDecaf = true;
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
                result.FailedParses++;
                result.exceptions.Add(ex);
            }
        }

        result.IsSuccessful = true;
        result.Listings = listings;

        return result;
    }
}