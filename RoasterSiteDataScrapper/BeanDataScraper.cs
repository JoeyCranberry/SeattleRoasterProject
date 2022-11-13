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
        private const string recordFilePath = @"C:\Users\JoeMini\source\repos\SeattleRoasterProject\RoasterSiteDataScrapper\ShopArchive\record.json";
        private const int archiveStaleAfterDays = 7;

        public async static Task<List<BeanModel>> GetRoasterBeans(RoasterModel roaster)
		{
            // Get the archive record
            ArchiveRecord? roasterArchive = GetArchiveRecord(roaster);
            if(roasterArchive == null)
			{
                return new List<BeanModel>();
			}

            return roasterArchive.Beans;
        }

        public async static Task<List<BeanModel>?> GetNewRoasterBeans(RoasterModel roaster)
		{
			string? shopScrape = await GetPageContent(roaster.ShopURL);

			if (String.IsNullOrEmpty(shopScrape))
			{
                return null;
			}

			List<BeanModel> parsedListings = ParseListings(roaster, shopScrape);

            List<BeanModel> storedListings = await BeanAccess.GetBeansByRoaster(roaster);

            if(parsedListings.Count == storedListings.Count)
            {
                return null;
            }
            else
            {
                List<string> existingNames = new List<string>();
                foreach(BeanModel bean in storedListings)
                {
                    existingNames.Add(bean.FullName);
                }

                // Remove bean listings with names already stored
                parsedListings.RemoveAll(b => existingNames.Contains(b.FullName));

                return parsedListings;
            }
		}

        private static List<BeanModel> ParseListings(RoasterModel roaster, string shopScrape)
		{
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(shopScrape);

            List<BeanModel> listings = new List<BeanModel>();

            switch (roaster.Id)
            {
                case "636c4d4c720cf76568f2d200":
					listings = AnchorheadParser.ParseBeans(htmlDoc, roaster);
                    break;
				case "636c4d4c720cf76568f2d202":
					listings = ArmisticeParser.ParseBeans(htmlDoc, roaster);
					break;
			}

            return listings;
        }

		private static async Task<string?> GetPageContent(string path)
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
            // Wait for a bit
            await page.EvaluateExpressionAsync("new Promise(function(resolve) { setTimeout(resolve, 100)});");

            var content = await page.GetContentAsync();

            return content;
        }

       
        private static void SaveRoasterRecord(ArchiveRecord record)
		{
            // Retrieve the record of archived pages
            ArchiveRecordObject? archive = GetArchive(recordFilePath);

            if(archive != null && archive.Records != null)
			{
                // Add the new page
                archive.Records.Add(record);

                // Save the record
                File.WriteAllText(recordFilePath, JsonConvert.SerializeObject(archive));
            }
        }

        private static ArchiveRecord? GetArchiveRecord(RoasterModel roaster)
        {
            ArchiveRecordObject? archive = GetArchive(recordFilePath);

            if (archive == null || archive.Records == null)
            {
                return null;
            }

            List<ArchiveRecord> roasterRecords = archive.Records.Where(r => r.RoasterId == roaster.RoasterId).OrderBy(r => r.LastUpdated).ToList();

            if (roasterRecords.Count > 0)
            {
                return roasterRecords[0];
            }
            else
			{
                return null;
			}

        }

        public static ArchiveRecordObject? GetArchive(string filePath)
		{
            ArchiveRecordObject deserialized;

            try
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    string json = reader.ReadToEnd();
                    deserialized = JsonConvert.DeserializeObject<ArchiveRecordObject>(json);
                }
            }
            catch
            {
                return null;
            }

            return deserialized;
        }
    }

    public class ArchiveRecordObject
	{
        public List<ArchiveRecord>? Records;
	}

    public class ArchiveRecord
	{
        public int RoasterId { get; set; }
        public DateTime LastUpdated { get; set; }
        public List<BeanModel> Beans { get; set; }
    }
}