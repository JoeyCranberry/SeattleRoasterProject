using PuppeteerSharp;
using RoasterBeansDataAccess.Models;

namespace RoasterBeansDataAccess.DataAccess
{
	internal class PageContentAccess
	{
		public static async Task<string?> GetPageContent(string path, int waitPageLoadTimes = 0, int waitTimeMilliseconds = 0 )
		{
            using var browserFetcher = new BrowserFetcher();
            await browserFetcher.DownloadAsync();
			var browser = await Puppeteer.LaunchAsync(new LaunchOptions
			{
				Headless = true,
				Timeout = 30000 * (waitPageLoadTimes + 1),
				Args = new string[]{ "--no-zygote", "--no-sandbox" }
			});
			var page = await browser.NewPageAsync();

			try
			{
				await page.GoToAsync(path);
			}
			catch(PuppeteerSharp.NavigationException ex)
			{
				Console.WriteLine("PuppeteerSharp NavigationException suppressed when navigating to path: " + path);
				Console.WriteLine(ex);
				browserFetcher.Dispose();
				await browser.CloseAsync();

				return String.Empty;
			}

			// Scroll to the bottom
			try
			{
				await page.EvaluateExpressionAsync("window.scrollTo(0, document.body.scrollHeight);");
			}
			catch(Exception exc)
			{
				Console.WriteLine("Error in initial scroll: ");
				Console.WriteLine(exc.Message);
				return null;
			}
			
			for (int i = 0; i < waitPageLoadTimes; i++)
			{
				try
				{
					await page.WaitForNavigationAsync(new NavigationOptions() { Timeout = 30000 });
					await page.EvaluateExpressionAsync("window.scrollTo(0, document.body.scrollHeight);");
				}
				catch (System.TimeoutException ex)
				{
					Console.WriteLine("Timeout suppressed when fetching page content for " + path);
					Console.WriteLine(ex);
					continue;
				}
			}

			Thread.Sleep(waitTimeMilliseconds);

			var content = await page.GetContentAsync();

			browserFetcher.Dispose();
			await browser.CloseAsync();

			return content;
		}
	}

	public class ParseContentResult
	{
		public bool IsSuccessful { get; set; }
		public List<BeanModel>? Listings { get; set; }
		public List<Exception> exceptions { get; set; } = new();
		public int FailedParses { get; set; }
	}
		
}
