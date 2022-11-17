using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using RoasterBeansDataAccess.Parsers;
using PuppeteerSharp;
using Newtonsoft.Json;
using RoasterBeansDataAccess.Models;
using RoasterBeansDataAccess.DataAccess;

namespace RoasterBeansDataAccess
{
    public static class BeanDataScraper
    {
        public async static Task<(List<BeanModel>? newListings, List<BeanModel>? removedListings)> GetNewRoasterBeans(RoasterModel roaster)
		{
			string? shopScrape = await GetPageContent(roaster.ShopURL);

			if (String.IsNullOrEmpty(shopScrape))
			{
                return (null, null);
			}

			List<BeanModel> parsedListings = await ParseListings(roaster, shopScrape);

            List<BeanModel> storedListings = await BeanAccess.GetBeansByRoaster(roaster);

            List<BeanModel> removedListings = storedListings;
            List<BeanModel> newListings = parsedListings;


			if (parsedListings.Count == storedListings.Count)
            {
                return (null, null);
            }
            else
            {
                // Get list of stored product URLs to compare against
                List<string> storedProductURLs = new List<string>();
                foreach(BeanModel bean in storedListings)
                {
                    storedProductURLs.Add(bean.ProductURL);
                }


				// Get list of parsed product URLs to compare against
				List<string> parsedProductURLs = new List<string>();
				foreach (BeanModel bean in parsedListings)
				{
					parsedProductURLs.Add(bean.ProductURL);
				}

                // Add any listings where they exist in stored listings but not parsed listings
                removedListings.AddRange(storedListings.Where(b => !parsedProductURLs.Contains(b.ProductURL)));

				// Removed any listings from parsed listings where product URL is already stored
				newListings.RemoveAll(b => storedProductURLs.Contains(b.ProductURL));

                return new (newListings, removedListings);
            }
		}

        private static async Task<List<BeanModel>> ParseListings(RoasterModel roaster, string shopScrape)
		{
            HtmlDocument htmlDoc = new HtmlDocument();
            
            List<BeanModel> listings = new List<BeanModel>();

            switch (roaster.Id)
            {
                case "636c4d4c720cf76568f2d200":
					htmlDoc.LoadHtml(shopScrape);
					listings = AnchorheadParser.ParseBeans(htmlDoc, roaster);
                    break;
				case "636c4d4c720cf76568f2d202":
					htmlDoc.LoadHtml(shopScrape);
					listings = ArmisticeParser.ParseBeans(htmlDoc, roaster);
					break;
                case "636c4d4c720cf76568f2d203":
					htmlDoc.LoadHtml(shopScrape);
					listings = AvoleParser.ParseBeans(htmlDoc, roaster); 
                    break;
                case "636c4d4c720cf76568f2d21e":
					htmlDoc.LoadHtml(shopScrape);
					listings = BatdorfBronsonParser.ParseBeans(htmlDoc, roaster);
                    break;
                case "636c4d4c720cf76568f2d201":
                    listings = await BellinghamParser.ParseBeans(roaster);
                    break;
                case "636c4d4c720cf76568f2d204":
					htmlDoc.LoadHtml(shopScrape);
					listings = BlackCoffeeParser.ParseBeans(htmlDoc, roaster);
                    break;
                case "636c4d4c720cf76568f2d205":
					htmlDoc.LoadHtml(shopScrape);
					listings = BluebeardParser.ParseBeans(htmlDoc, roaster); 
                    break;
                case "636c4d4c720cf76568f2d206":
					htmlDoc.LoadHtml(shopScrape);
					listings = BoonBoonaParser.ParseBeans(htmlDoc, roaster); 
                    break;
                case "636c4d4c720cf76568f2d207":
					htmlDoc.LoadHtml(shopScrape);
					listings = BroadcastParser.ParseBeans(htmlDoc, roaster);
                    break;
                case "637567f889596e3d71617703":
                    listings = await BlossomParser.ParseBeans(roaster);
                    break;
                case "636c4d4c720cf76568f2d208":
					htmlDoc.LoadHtml(shopScrape);
					listings = CafeAllegroParser.ParseBeans(htmlDoc, roaster);
                    break;
                case "636c4d4c720cf76568f2d20b":
                    htmlDoc.LoadHtml(shopScrape);
                    listings = CaffeLadroParser.ParseBeans(htmlDoc, roaster);
                    break;
                case "636c4d4c720cf76568f2d20c":
					htmlDoc.LoadHtml(shopScrape);
					listings = CaffeLussoParser.ParseBeans(htmlDoc, roaster);
					break;
                case "636c4d4c720cf76568f2d20d":
					htmlDoc.LoadHtml(shopScrape);
					listings = CaffeUmbriaParser.ParseBeans(htmlDoc, roaster);
					break;
			}

            return listings;
        }

		public static async Task<string?> GetPageContent(string path)
		{
            using var browserFetcher = new BrowserFetcher();
            await browserFetcher.DownloadAsync(BrowserFetcher.DefaultChromiumRevision);
            var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true
            });
            var page = await browser.NewPageAsync();
            await page.GoToAsync(path);
            // Scroll to the bottom
            await page.EvaluateExpressionAsync("window.scrollBy(0, window.innerHeight)");
            //// Wait for a bit
            //await page.EvaluateExpressionAsync("new Promise(function(resolve) { setTimeout(resolve, 100)});");

            var content = await page.GetContentAsync();

            return content;
        }
    }
}