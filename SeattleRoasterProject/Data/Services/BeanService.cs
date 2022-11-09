using RoasterBeansDataAccess;

namespace SeattleRoasterProject.Data.Services
{
	public class BeanService
	{
		public async Task<List<BeanListing>> GetBeans(Roaster roaster)
		{
			await BeanDataScraper.UpdateRoasterBeanListing(roaster);
			return await BeanDataScraper.GetRoasterBeans(roaster);
		}

		public async Task<bool> CheckForUpdate(Roaster roaster)
        {
			return await BeanDataScraper.UpdateRoasterBeanListing(roaster);
		}
	}
}
