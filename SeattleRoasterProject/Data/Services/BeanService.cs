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

		public async Task<BeanGetResult> GetBeansByFilter(BeanFilter filter)
		{
			return await BeanAccess.GetBeansByFilter(filter);
		}

		public async Task<List<BeanModel>> GetBeansByIds(List<string> beanIds)
		{
			return await BeanAccess.GetAllBeansByIds(beanIds);
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

		public async Task<bool> UnsetField(string fieldName)
		{
			return await BeanAccess.UnsetField(fieldName);
		}
	}

	public class BeanListingModel
	{
		public BeanModel Bean { get; set; }
		public RoasterModel Roaster { get; set; }
	}
}
