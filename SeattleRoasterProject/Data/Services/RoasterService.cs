using RoasterBeansDataAccess;
using RoasterBeansDataAccess.Models;
using RoasterBeansDataAccess.DataAccess;

namespace SeattleRoasterProject.Data.Services
{
    public class RoasterService
    {
        private readonly EnvironmentSettings _environmentSettings;
        private readonly bool _isDevelopment = false;

		public RoasterService(EnvironmentSettings environmentSettings)
		{
			_environmentSettings = environmentSettings;
			_isDevelopment = environmentSettings.GetEnvironment() == EnvironmentSettings.Environment.Development;
		}

		public async Task<List<RoasterModel>> GetAllRoasters()
        {
            var roasters = await RoasterAccess.GetAllRoasters(_isDevelopment);

            return roasters.OrderBy(r => r.Name).ToList();
        }

		public async Task<List<RoasterModel>> GetAllRoastersByEnvironment(EnvironmentSettings.Environment env)
		{
			var roasters = await RoasterAccess.GetAllRoasters(_isDevelopment);
            if(env != EnvironmentSettings.Environment.Development)
            {
                roasters.RemoveAll(r => r.RecievedPermission);
            }

            return roasters.OrderBy(r => r.Name).ToList();
		}

		public async Task<RoasterModel> GetRoasterByMongoId(string id)
		{
			var roasterMatch = await RoasterAccess.GetRoasterById(id, _isDevelopment);

			return roasterMatch ?? new RoasterModel();
		}

        public async Task<List<RoasterModel>> GetRoastersByName(string name, EnvironmentSettings.Environment env)
        {
            List<RoasterModel> results = new List<RoasterModel>();

            string[] terms = name.Split(' ');
            foreach(string term in terms)
            {
                var roasterMatch = await RoasterAccess.GetRoastersByName(term, _isDevelopment);
                if (roasterMatch != null)
                {
                    results.AddRange(roasterMatch);
				}
			}

            return results;

		}

        public async Task<bool> AddRoasterToDb(RoasterModel newRoaster, EnvironmentSettings.Environment env)
		{
            return await RoasterAccess.AddRoaster(newRoaster, _isDevelopment);
        }

        public async Task<bool> ReplaceRoasterInDb(RoasterModel oldRoaster, RoasterModel newRoaster, EnvironmentSettings.Environment env)
        {
            return await RoasterAccess.ReplaceRoaster(oldRoaster, newRoaster, _isDevelopment);
        }

        public async Task<bool> DeleteRoasterInDb(RoasterModel delRoaster)
        {
            return await RoasterAccess.DeleteRoaster(delRoaster, _isDevelopment);
        }

        public async Task<BeanListingDifference> CheckForUpdate(RoasterModel roaster)
        {
            BeanListingDifference results = await BeanDataScraper.GetBeanListingDifference(roaster);

            return results;
        }
    }
}
