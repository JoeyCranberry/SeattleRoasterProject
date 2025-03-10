﻿using HtmlAgilityPack;
using RoasterBeansDataAccess.DataAccess;
using RoasterBeansDataAccess.Models;

namespace RoasterBeansDataAccess.Parsers;

public class FulcrumParser
{
    private static readonly List<string> excludedTerms = new()
        { "egift", "cold brew", "coffee tin", "tumbler", "gift certificate", "package", "sample" };


    public static async Task<ParseContentResult> ParseBeansForRoaster(RoasterModel roaster)
    {
        var overallResult = new ParseContentResult
        {
            Listings = new List<BeanModel>(),
            IsSuccessful = false
        };

        // Get Fulcrum
        overallResult = await ParsePage(overallResult, "https://fulcrumcoffee.com/product-category/coffees/fulcrum/",
            roaster, 1);
        // Get Silver Cup
        overallResult = await ParsePage(overallResult, "https://fulcrumcoffee.com/product-category/coffees/silver-cup/",
            roaster, 0);
        // Get Urban City
        overallResult = await ParsePage(overallResult, "https://fulcrumcoffee.com/product-category/coffees/urban-city/",
            roaster, 0);

        return overallResult;
    }

    private static async Task<ParseContentResult> ParsePage(ParseContentResult overallResult, string pageURL,
        RoasterModel roaster, int waitTimes)
    {
        var shopContent = await PageContentAccess.GetPageContent(pageURL, waitTimes);
        if (!string.IsNullOrEmpty(shopContent))
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(shopContent);

            var parseResult = ParseBeans(htmlDoc, roaster);

            if (parseResult.IsSuccessful && parseResult.Listings != null && overallResult.Listings != null)
            {
                overallResult.Listings.AddRange(parseResult.Listings);
                overallResult.FailedParses += parseResult.FailedParses;
                overallResult.IsSuccessful = true;
            }
        }

        return overallResult;
    }

    private static ParseContentResult ParseBeans(HtmlDocument shopHTML, RoasterModel roaster)
    {
        var result = new ParseContentResult();

        var shopParent = shopHTML.DocumentNode.SelectSingleNode("//div[@class='ae-post-list-wrapper']");
        if (shopParent == null)
        {
            result.IsSuccessful = false;
            return result;
        }

        List<HtmlNode>? shopItems = shopParent.SelectNodes("./article")?.ToList();
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
                var imageURL = productListing.SelectSingleNode(".//img[contains(@class, 'attachment-')]")
                    .GetAttributeValue("src", "");

                var productURL = productListing.SelectSingleNode(".//a").GetAttributeValue("href", "");

                listing.ImageURL = imageURL;
                listing.ProductURL = productURL;

                var name = productListing.SelectSingleNode(".//div[contains(@class, 'elementor-text-editor')]")
                    .InnerText.Trim();
                listing.FullName = name;

                var priceNode = productListing.SelectSingleNode(".//span[contains(@class, 'woocommerce-Price-amount')]")
                    ?.SelectSingleNode("./bdi");
                if (priceNode != null)
                {
                    var price = priceNode.InnerText.Replace("$", "").Trim();

                    decimal parsedPrice;
                    if (decimal.TryParse(price, out parsedPrice))
                    {
                        listing.PriceBeforeShipping = parsedPrice;
                    }
                }

                listing.AvailablePreground = false;
                listing.SizeOunces = 12;

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