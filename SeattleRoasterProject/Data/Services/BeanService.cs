using RoasterBeansDataAccess.Models;
using RoasterBeansDataAccess.DataAccess;
using System.Net;

namespace SeattleRoasterProject.Data.Services
{
    public class BeanService
	{
		private readonly EnvironmentSettings _environmentSettings;
		private readonly bool _isDevelopment = false;

		public BeanService(EnvironmentSettings environmentSettings)
		{
			_environmentSettings = environmentSettings;
			_isDevelopment = environmentSettings.IsDevelopment;
		}

		public async Task<List<BeanModel>> GetAllBeans()
		{
			return await BeanAccess.GetAllBeans(_isDevelopment);
		}

		public async Task<List<BeanModel>> GetAllBeansWithBrokenImageLink()
		{
			var allBeans = await BeanAccess.GetAllBeansNotExcluded(_isDevelopment);

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
				var request = WebRequest.Create(url) as HttpWebRequest;
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

		public async Task<BeanGetResult> GetBeansByFilter(BeanFilter filter)
		{
			return await BeanAccess.GetBeansByFilter(filter, _isDevelopment);
		}

		public async Task<List<BeanModel>> GetBeansByIds(List<string> beanIds)
		{
			return await BeanAccess.GetAllBeansByIds(beanIds, _isDevelopment);
		}

		public async Task<List<BeanModel>> GetAllProductionInvisibleBeans()
		{
			return await BeanAccess.GetAllProductionInvisibleBeans(_isDevelopment);
		}

		public async Task<bool> AddBeanToDb(BeanModel newBean)
        {
			return await BeanAccess.AddBean(newBean, _isDevelopment);
        }

		public async Task<bool> UpdateExistingBean(BeanModel editBean)
        {
			return await BeanAccess.UpdateBean(editBean, _isDevelopment);
        }

		public async Task<bool> SetBeanToInactive(BeanModel bean)
		{
			bean.IsActiveListing = false;
			return await BeanAccess.UpdateBean(bean, _isDevelopment);
		}

		public async Task<bool> DeleteBean(BeanModel delBean)
        {
			return await BeanAccess.DeleteBean(delBean, _isDevelopment);
		}

		public async Task<bool> UnsetField(string fieldName)
		{
			return await BeanAccess.UnsetField(fieldName, _isDevelopment);
		}
	}

	public class BeanListingModel
	{
		public BeanModel Bean { get; set; }
		public RoasterModel Roaster { get; set; }
	}
}
