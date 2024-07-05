using RoasterBeansDataAccess;
using RoasterBeansDataAccess.Models;
using RoasterBeansDataAccess.DataAccess;
using SeattleRoasterProject;
using MongoDB.Driver;

namespace SeattleRoasterProject.Data.Services
{
    public class RoasterService
    {
        public async Task<List<RoasterModel>> GetAllRoasters(EnvironmentSettings.Environment env)
        {
            var roasters = await RoasterAccess.GetAllRoasters(env == EnvironmentSettings.Environment.Development);

            return roasters.OrderBy(r => r.Name).ToList() ?? new List<RoasterModel>();
        }

		public async Task<List<RoasterModel>> GetAllRoastersbyEnviroment(EnvironmentSettings.Environment env)
		{
			var roasters = await RoasterAccess.GetAllRoasters(env == EnvironmentSettings.Environment.Development);
            if(env != EnvironmentSettings.Environment.Development)
            {
                roasters.RemoveAll(r => r.RecievedPermission);
            }
            
			return roasters.OrderBy(r => r.Name).ToList() ?? new List<RoasterModel>();
		}

		public async Task<RoasterModel> GetRoasterByMongoId(string id, EnvironmentSettings.Environment env)
		{
			var roasterMatch = await RoasterAccess.GetRoasterById(id, env == EnvironmentSettings.Environment.Development);

			return roasterMatch ?? new RoasterModel();
		}

        public async Task<List<RoasterModel>> GetRoastersByName(string name, EnvironmentSettings.Environment env)
        {
            List<RoasterModel> results = new List<RoasterModel>();

            string[] terms = name.Split(' ');
            foreach(string term in terms)
            {
                var roasterMatch = await RoasterAccess.GetRoastersByName(term, env == EnvironmentSettings.Environment.Development);
                if (roasterMatch != null)
                {
                    results.AddRange(roasterMatch);
				}
			}

            return results;

		}

        public async Task<bool> AddRoasterToDb(RoasterModel newRoaster, EnvironmentSettings.Environment env)
		{
            return await RoasterAccess.AddRoaster(newRoaster, env == EnvironmentSettings.Environment.Development);
        }

        public async Task<bool> ReplaceRoasterInDb(RoasterModel oldRoaster, RoasterModel newRoaster, EnvironmentSettings.Environment env)
        {
            return await RoasterAccess.ReplaceRoaster(oldRoaster, newRoaster, env == EnvironmentSettings.Environment.Development);
        }

        public async Task<bool> DeleteRoasterInDb(RoasterModel delRoaster, EnvironmentSettings.Environment env)
        {
            return await RoasterAccess.DeleteRoaster(delRoaster, env == EnvironmentSettings.Environment.Development);
        }

        public async Task<BeanListingDifference> CheckForUpdate(RoasterModel roaster, EnvironmentSettings.Environment env)
        {
            BeanListingDifference results = await BeanDataScraper.GetBeanListingDifference(roaster);

            return results;
        }
    }
}
