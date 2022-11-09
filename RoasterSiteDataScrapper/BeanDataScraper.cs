using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoasterBeansDataAccess;
using HtmlAgilityPack;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using RoasterBeansDataAccess.Parsers;
using PuppeteerSharp;
using Newtonsoft.Json;

namespace RoasterBeansDataAccess
{
    public static class BeanDataScraper
    {
        private const string recordFilePath = @"C:\Users\JoeMini\source\repos\SeattleRoasterProject\RoasterSiteDataScrapper\ShopArchive\record.json";
        private const int archiveStaleAfterDays = 7;

        public async static Task<List<BeanListing>> GetRoasterBeans(Roaster roaster)
		{
            // Get the archive record
            ArchiveRecord? roasterArchive = GetArchiveRecord(roaster);
            if(roasterArchive == null)
			{
                return new List<BeanListing>();
			}

            return roasterArchive.Beans;
        }

        public async static Task<bool> UpdateRoasterBeanListing(Roaster roaster)
		{
            // Get the archive record
            ArchiveRecord? roasterRecord = GetArchiveRecord(roaster);

            // Check if the record is null or hasn't been updated in a while
            if(roasterRecord == null || (DateTime.Now - roasterRecord.LastUpdated).Days >= archiveStaleAfterDays)
			{

                string? shopScrape = await GetPageContent(roaster.ShopURL);
            }

            return true;
        }

        public async static Task<List<BeanListing>> ScrapeBeans(Roaster roaster)
        {
            DateTime? lastTimeUpdated = GetArchiveRecord(roaster)?.LastUpdated;

            // Check if the archive needs an update
            if(lastTimeUpdated == null || (DateTime.Now - lastTimeUpdated).Value.Days >= archiveStaleAfterDays)
			{
                // If it does - use Headless chrome to get the page content
                string? shopScrape = await GetPageContent(roaster.ShopURL);
                
                // Save the content to the archive
                if(shopScrape != null)
				{
                    await SaveShopPageToArchive(shopScrape, roaster);

                    // Parse the html and retrieve the bean listings
                    List<BeanListing> listings = ParseListings(roaster, shopScrape);
                }
                else
				{

				}
            }
            else
			{

			}

            return new();
        }

        private static List<BeanListing> ParseListings(Roaster roaster, string shopScrape)
		{
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(shopScrape);

            List<BeanListing> listings = new List<BeanListing>();

            switch (roaster.RoasterId)
            {
                case 0:
                    listings = AnchorheadParser.ParseBeans(htmlDoc);
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
                Headless = false
            });
            var page = await browser.NewPageAsync();
            await page.GoToAsync(path);

            var content = await page.GetContentAsync();

            return content;
        }

        private static async Task SaveShopPageToArchive(string scrapedPage, Roaster roaster)
		{
            // Create copy of the shop page and save it as a .html
            string filePath = @"C:\Users\JoeMini\source\repos\SeattleRoasterProject\RoasterSiteDataScrapper\ShopArchive\" + roaster.Name + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".html";
            await File.WriteAllTextAsync(filePath, scrapedPage);

            // Retrieve the record of archived pages
            ArchiveRecordObject? archive = GetArchive(recordFilePath);

            if(archive != null && archive.Records != null)
			{
                // Add the new page
                archive.Records.Add(new ArchiveRecord()
                {
                    RoasterId = roaster.RoasterId,
                    FilePath = filePath,
                    LastUpdated = DateTime.Now
                });

                // Save the record
                File.WriteAllText(recordFilePath, JsonConvert.SerializeObject(archive));
            }
        }

        private static ArchiveRecord? GetArchiveRecord(Roaster roaster)
        {
            ArchiveRecordObject? archive = GetArchive(recordFilePath);

            if (archive == null || archive.Records == null)
            {
                return null;
            }

            List<ArchiveRecord> roasterRecords = archive.Records.Where(r => r.RoasterId == roaster.RoasterId).OrderBy(r => r.LastUpdated).OrderBy(r => r.LastUpdated).ToList();

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
        public string FilePath { get; set; } = String.Empty;
        public List<BeanListing> Beans { get; set; }
    }
}