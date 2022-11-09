using RoasterBeansDataAccess;

namespace SeattleRoasterProject.Data.Services
{
	public class BeanService
	{
		public async Task GetBeans(Roaster roaster)
		{
			await BeanDataScraper.ScrapeBeans(roaster);
		}
	}
}
