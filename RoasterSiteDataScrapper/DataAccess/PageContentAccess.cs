using PuppeteerSharp;
using RoasterBeansDataAccess.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoasterBeansDataAccess.DataAccess
{
	internal class PageContentAccess
	{
		public static async Task<string?> GetPageContent(string path, int waitPageLoadTimes = 0)
		{
			using var browserFetcher = new BrowserFetcher();
			await browserFetcher.DownloadAsync(BrowserFetcher.DefaultChromiumRevision);
			var browser = await Puppeteer.LaunchAsync(new LaunchOptions
			{
				Headless = true,
				Timeout = 30000 * (waitPageLoadTimes + 1)
			});
			var page = await browser.NewPageAsync();

			await page.GoToAsync(path);
			// Scroll to the bottom
			await page.EvaluateExpressionAsync("window.scrollTo(0, document.body.scrollHeight);");

			for (int i = 0; i < waitPageLoadTimes; i++)
			{
				try
				{
					await page.WaitForNavigationAsync(new NavigationOptions() { Timeout = 30000 });
					await page.EvaluateExpressionAsync("window.scrollTo(0, document.body.scrollHeight);");
				}
				catch (System.TimeoutException ex)
				{
					continue;
				}
			}

			var content = await page.GetContentAsync();

			browserFetcher.Dispose();

			return content;
		}
	}

	public class ParseContentResult
	{
		public bool IsSuccessful { get; set; }
		public List<BeanModel>? Listings { get; set; }
		public List<Exception>? exceptions { get; set; }	
		public int FailedParses { get; set; }
	}
		
}
