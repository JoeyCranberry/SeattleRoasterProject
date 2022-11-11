using RoasterBeansDataAccess;
using RoasterBeansDataAccess.Models;
using RoasterBeansDataAccess.DataAccess;

namespace SeattleRoasterProject.Data.Services
{
    public class BeanService
	{
		public async Task<List<BeanModel>> GetBeans()
		{
			return await BeanAccess.GetAllBeansNotExcluded();
		}

		public async Task<List<BeanModel>> GetSingleOrginEthiopianBeans()
		{
			return await BeanAccess.GetBeansByFilter(new BeanFilter()
			{
				IsSingleOrigin = new FilterValueBool(true, true),
				IsExcluded = new FilterValueBool(true, false),
				CountryFilter = new FilterList<Country>(true, new List<Country>{ Country.ETHIOPIA }) 
			});
		}

		public async Task<bool> AddBeanToDb(BeanModel newBean)
        {
			return await BeanAccess.AddBean(newBean);
        }

		public async Task<bool> UpdateExistingBean(BeanModel editBean)
        {
			return await BeanAccess.UpdateBean(editBean);
        }

		public async Task<bool> DeleteBean(BeanModel delBean)
        {
			return await BeanAccess.DeleteBean(delBean);

		}


		public async Task<bool> CheckForUpdate(RoasterModel roaster)
        {
			return await BeanDataScraper.UpdateRoasterBeanListing(roaster);
		}
	}
}
