using RoasterBeansDataAccess;
using RoasterBeansDataAccess.Models;
using RoasterBeansDataAccess.DataAccess;
using SeattleRoasterProject;
using MongoDB.Driver;

namespace SeattleRoasterProject.Data.Services
{
    public class RoasterService
    {
        public async Task<List<RoasterModel>> GetAllRoasters(EnviromentSettings.Enviroment env)
        {
            var roasters = await RoasterAccess.GetAllRoasters(env == EnviromentSettings.Enviroment.DEVELOPMENT);

            return roasters.OrderBy(r => r.Name).ToList() ?? new List<RoasterModel>();
        }

		public async Task<List<RoasterModel>> GetAllRoastersbyEnviroment(EnviromentSettings.Enviroment env)
		{
			var roasters = await RoasterAccess.GetAllRoasters(env == EnviromentSettings.Enviroment.DEVELOPMENT);
            if(env != EnviromentSettings.Enviroment.DEVELOPMENT)
            {
                roasters.RemoveAll(r => r.RecievedPermission);
            }
            
			return roasters.OrderBy(r => r.Name).ToList() ?? new List<RoasterModel>();
		}

		public async Task<RoasterModel> GetRoasterByMongoId(string id, EnviromentSettings.Enviroment env)
		{
			var roasterMatch = await RoasterAccess.GetRoasterById(id, env == EnviromentSettings.Enviroment.DEVELOPMENT);

			return roasterMatch ?? new RoasterModel();
		}

        public async Task<List<RoasterModel>> GetRoastersByName(string name, EnviromentSettings.Enviroment env)
        {
            List<RoasterModel> results = new List<RoasterModel>();

            string[] terms = name.Split(' ');
            foreach(string term in terms)
            {
                var roasterMatch = await RoasterAccess.GetRoastersByName(term, env == EnviromentSettings.Enviroment.DEVELOPMENT);
                if (roasterMatch != null)
                {
                    results.AddRange(roasterMatch);
				}
			}

            return results;

		}

        public async Task<bool> AddRoasterToDb(RoasterModel newRoaster, EnviromentSettings.Enviroment env)
		{
            return await RoasterAccess.AddRoaster(newRoaster, env == EnviromentSettings.Enviroment.DEVELOPMENT);
        }

        public async Task<bool> ReplaceRoasterInDb(RoasterModel oldRoaster, RoasterModel newRoaster, EnviromentSettings.Enviroment env)
        {
            return await RoasterAccess.ReplaceRoaster(oldRoaster, newRoaster, env == EnviromentSettings.Enviroment.DEVELOPMENT);
        }

        public async Task<bool> DeleteRoasterInDb(RoasterModel delRoaster, EnviromentSettings.Enviroment env)
        {
            return await RoasterAccess.DeleteRoaster(delRoaster, env == EnviromentSettings.Enviroment.DEVELOPMENT);
        }

        public async Task<BeanListingDifference> CheckForUpdate(RoasterModel roaster, EnviromentSettings.Enviroment env)
        {
            BeanListingDifference results = await BeanDataScraper.GetBeanListingDifference(roaster);

            return results;
        }
    }
}
