using RoasterBeansDataAccess;
using RoasterBeansDataAccess.Models;
using RoasterBeansDataAccess.DataAccess;

namespace SeattleRoasterProject.Data.Services
{
    public class BeanService
	{
		public async Task<BeanGetResult> GetBeansByFilter(BeanFilter filter, EnvironmentSettings.Environment env)
		{
			return await BeanAccess.GetBeansByFilter(filter, env == EnvironmentSettings.Environment.DEVELOPMENT);
		}

		public async Task<List<BeanModel>> GetBeansByIds(List<string> beanIds, EnvironmentSettings.Environment env)
		{
			return await BeanAccess.GetAllBeansByIds(beanIds, env == EnvironmentSettings.Environment.DEVELOPMENT);
		}

		public async Task<List<BeanModel>> GetAllProductionInvisibleBeans(EnvironmentSettings.Environment env)
		{
			return await BeanAccess.GetAllProductionInvisibleBeans(env == EnvironmentSettings.Environment.DEVELOPMENT);
		}

		public async Task<bool> AddBeanToDb(BeanModel newBean, EnvironmentSettings.Environment env)
        {
			return await BeanAccess.AddBean(newBean, env == EnvironmentSettings.Environment.DEVELOPMENT);
        }

		public async Task<bool> UpdateExistingBean(BeanModel editBean, EnvironmentSettings.Environment env)
        {
			return await BeanAccess.UpdateBean(editBean, env == EnvironmentSettings.Environment.DEVELOPMENT);
        }

		public async Task<bool> DeleteBean(BeanModel delBean, EnvironmentSettings.Environment env)
        {
			return await BeanAccess.DeleteBean(delBean, env == EnvironmentSettings.Environment.DEVELOPMENT);
		}

		public async Task<bool> UnsetField(string fieldName, EnvironmentSettings.Environment env)
		{
			return await BeanAccess.UnsetField(fieldName, env == EnvironmentSettings.Environment.DEVELOPMENT);
		}
	}

	public class BeanListingModel
	{
		public BeanModel Bean { get; set; }
		public RoasterModel Roaster { get; set; }
	}
}
