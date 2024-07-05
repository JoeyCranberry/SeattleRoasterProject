using RoasterBeansDataAccess;
using RoasterBeansDataAccess.Models;
using RoasterBeansDataAccess.DataAccess;
using System.Net;

namespace SeattleRoasterProject.Data.Services
{
    public class BeanService
	{
		public async Task<List<BeanModel>> GetAllBeans(EnvironmentSettings.Environment env)
		{
			return await BeanAccess.GetAllBeans(env == EnvironmentSettings.Environment.DEVELOPMENT);
		}

		public async Task<List<BeanModel>> GetAllBeansWithBrokenImageLink(EnvironmentSettings.Environment env)
		{
			var allBeans = await BeanAccess.GetAllBeansNotExcluded(env == EnvironmentSettings.Environment.DEVELOPMENT);

			if(allBeans == null) 
			{
				return new();
			}

			var beansWithBrokenImageLink = new List<BeanModel>();

			foreach (var bean in allBeans) 
			{ 
				if(!RemoteFileExists(bean.ImageURL))
				{
					beansWithBrokenImageLink.Add(bean);
				}
			}

			return beansWithBrokenImageLink;
		}

		private bool RemoteFileExists(string url)
		{
			try
			{
				//Creating the HttpWebRequest
				HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
				//Setting the Request method HEAD, you can also use GET too.
				request.Method = "HEAD";
				//Getting the Web Response.
				HttpWebResponse response = request.GetResponse() as HttpWebResponse;
				//Returns TRUE if the Status code == 200
				response.Close();
				return (response.StatusCode == HttpStatusCode.OK);
			}
			catch
			{
				//Any exception will returns false.
				return false;
			}
		}

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

		public async Task<bool> SetBeanToInactive(BeanModel bean, EnvironmentSettings.Environment env)
		{
			bean.IsActiveListing = false;
			return await BeanAccess.UpdateBean(bean, env == EnvironmentSettings.Environment.DEVELOPMENT);
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
