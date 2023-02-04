using RoasterBeansDataAccess;
using RoasterBeansDataAccess.Models;
using RoasterBeansDataAccess.DataAccess;

namespace SeattleRoasterProject.Data.Services
{
    public class BeanService
	{
		public async Task<BeanGetResult> GetBeansByFilter(BeanFilter filter, EnviromentSettings.Enviroment env)
		{
			return await BeanAccess.GetBeansByFilter(filter, env == EnviromentSettings.Enviroment.DEVELOPMENT);
		}

		public async Task<List<BeanModel>> GetBeansByIds(List<string> beanIds, EnviromentSettings.Enviroment env)
		{
			return await BeanAccess.GetAllBeansByIds(beanIds, env == EnviromentSettings.Enviroment.DEVELOPMENT);
		}

		public async Task<List<BeanModel>> GetAllProductionInvisibleBeans(EnviromentSettings.Enviroment env)
		{
			return await BeanAccess.GetAllProductionInvisibleBeans(env == EnviromentSettings.Enviroment.DEVELOPMENT);
		}

		public async Task<bool> AddBeanToDb(BeanModel newBean, EnviromentSettings.Enviroment env)
        {
			return await BeanAccess.AddBean(newBean, env == EnviromentSettings.Enviroment.DEVELOPMENT);
        }

		public async Task<bool> UpdateExistingBean(BeanModel editBean, EnviromentSettings.Enviroment env)
        {
			return await BeanAccess.UpdateBean(editBean, env == EnviromentSettings.Enviroment.DEVELOPMENT);
        }

		public async Task<bool> DeleteBean(BeanModel delBean, EnviromentSettings.Enviroment env)
        {
			return await BeanAccess.DeleteBean(delBean, env == EnviromentSettings.Enviroment.DEVELOPMENT);
		}

		public async Task<bool> UnsetField(string fieldName, EnviromentSettings.Enviroment env)
		{
			return await BeanAccess.UnsetField(fieldName, env == EnviromentSettings.Enviroment.DEVELOPMENT);
		}
	}

	public class BeanListingModel
	{
		public BeanModel Bean { get; set; }
		public RoasterModel Roaster { get; set; }
	}
}
